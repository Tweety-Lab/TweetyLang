
using TweetyLang.AST;
using TweetyLang.Compiler;
using TweetyLang.Compiler.Symbols;
using TweetyLang.Parser.AST;

namespace TweetyLang.Parser.Semantics;

/// <summary>
/// Base class for semantic rules.
/// </summary>
internal abstract class BaseSemanticRule
{
    /// <summary> The compilation that is currently being analyzed. </summary>
    protected TweetyLangCompilation Compilation { get; private set; } = null!;

    /// <summary> A list of exceptions that occured during semantic analysis in this rule. </summary>
    internal List<SemanticError> Exceptions { get; } = new();

    /// <summary> A list of warnings that occured during semantic analysis in this rule. </summary>
    internal List<SemanticWarning> Warnings { get; } = new();

    /// <summary> Sets the compilation that is currently being analyzed. </summary>
    internal void SetCompilation(TweetyLangCompilation compilation) => Compilation = compilation;

    public virtual void AnalyzeFunction(FunctionNode func) { }
    public virtual void AnalyzeModule(ModuleNode module) { }
    public virtual void AnalyzeProgram(ProgramNode program) { }
    public virtual void AnalyzeStatement(StatementNode stmt) { }
    public virtual void AnalyzeExpression(ExpressionNode expr) { }

    /// <summary>
    /// Throws a semantic exception for a given AST node.
    /// </summary>
    protected void Error(AstNode node, string message)
    {
        Exceptions.Add(new SemanticError(node.SourceLine, node.SourceColumn, message));
    }

    /// <summary>
    /// Throws a semantic warning for a given AST node.
    /// </summary>
    protected void Warning(AstNode node, string message)
    {
        Warnings.Add(new SemanticWarning(node.SourceLine, node.SourceColumn, message));
    }
}

[SemanticAnalyzer]
internal class DuplicateParameterRule : BaseSemanticRule
{
    public override void AnalyzeFunction(FunctionNode func)
    {
        var paramNames = new HashSet<string>();
        foreach (var p in func.Parameters)
        {
            if (!paramNames.Add(p.Name))
                Error(func, $"Duplicate parameter '{p.Name}' in function '{func.Name}'.");
        }
    }
}

[SemanticAnalyzer]
internal class DuplicateFunctionRule : BaseSemanticRule
{
    public override void AnalyzeModule(ModuleNode module)
    {
        var functionNames = new HashSet<string>();
        foreach (var fn in module.Functions)
        {
            if (!functionNames.Add(fn.Name))
                Error(fn, $"Duplicate function '{fn.Name}' in module '{module.Name}'.");
        }
    }
}

[SemanticAnalyzer]
internal class FunctionCallVisibilityRule : BaseSemanticRule
{
    public static Dictionary<string, (string ModuleName, Modifiers Modifiers)> FunctionVisibility = new();

    public override void AnalyzeFunction(FunctionNode func)
    {
        // Find owning module
        var module = func.Ancestors().OfType<ModuleNode>().FirstOrDefault();
        if (module == null)
            throw new InvalidOperationException($"Function '{func.Name}' is not contained in a module.");

        FunctionVisibility[func.Name] = (module.Name, func.Modifiers);
    }

    public override void AnalyzeExpression(ExpressionNode expr)
    {
        if (expr is not FunctionCallNode call)
            return;

        // Check if function exists
        if (!FunctionVisibility.ContainsKey(call.Name))
        {
            Error(call, $"Tried to call unknown function '{call.Name}'.");
            return;
        }

        var (declaringModule, modifiers) = FunctionVisibility[call.Name];

        // Find the module of the current function call
        var currentModule = call.Ancestors().OfType<ModuleNode>().FirstOrDefault();
        if (currentModule == null)
        {
            Error(call, $"Function call '{call.Name}' is not inside any module... How?");
            return;
        }

        // Check module visibility: only allow calls to functions in the same module or imported modules
        var program = call.Ancestors().OfType<ProgramNode>().FirstOrDefault();
        bool isImported = program?.Imports.Any(i => i.ModuleName == declaringModule) ?? false;

        if (currentModule.Name != declaringModule && !isImported)
            Error(call, $"Cannot call function '{call.Name}' from module '{declaringModule}' because it is not imported.");

        // Check export modifier
        if (!modifiers.HasFlag(Modifiers.Export) && currentModule.Name != declaringModule)
            Error(call, $"Tried to call inaccessible function '{call.Name}' (not exported).");
    }
}

[SemanticAnalyzer]
internal class FunctionBodyRule : BaseSemanticRule
{
    public override void AnalyzeFunction(FunctionNode func)
    {
        if (func.Body == null && !func.Modifiers.HasFlag(Modifiers.Extern))
            Error(func, $"Non-externed function '{func.Name}' has no body.");
        
        if (func.Body != null && func.Modifiers.HasFlag(Modifiers.Extern))
            Error(func, $"Externed function '{func.Name}' has a body.");
    }
}

[SemanticAnalyzer]
internal class ImportRule : BaseSemanticRule
{
    public override void AnalyzeProgram(ProgramNode program)
    {
        var allModuleSymbols = Compilation.GetAllSymbols<IModuleSymbol>()
            .ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

        var seenImports = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var import in program.Imports)
        {
            // Duplicate import warning
            if (!seenImports.Add(import.ModuleName))
                Warning(import, $"Duplicate import of module '{import.ModuleName}'.");

            // Resolve imported module via symbol table
            if (!allModuleSymbols.ContainsKey(import.ModuleName))
                Error(import, $"Could not resolve import '{import.ModuleName}'.");
        }
    }
}

[SemanticAnalyzer]
internal class TypeDecAssignRule : BaseSemanticRule
{
    public override void AnalyzeStatement(StatementNode stmt)
    {
        switch (stmt)
        {
            case DeclarationNode decl:
                AnalyzeDeclaration(decl);
                break;

            case AssignmentNode assign:
                AnalyzeAssignment(assign);
                break;
        }
    }

    private void AnalyzeDeclaration(DeclarationNode decl)
    {
        var symbolDict = Compilation.GetSymbolDictionary(decl.Tree!);
        var variableSymbol = symbolDict.GetDeclaredSymbol<IVariableSymbol>(decl);

        if (variableSymbol == null)
        {
            Error(decl, $"No symbol found for variable '{decl.Name}'.");
            return;
        }

        var exprType = ResolveExpressionType(decl.Expression, variableSymbol.Type);

        if (exprType != variableSymbol.Type && exprType != new TypeReference("unknown"))
            Error(decl, $"Type mismatch: Expression type '{exprType}' does not match declared type '{variableSymbol.Type}'.");
    }


    private void AnalyzeAssignment(AssignmentNode assign)
    {
        var symbolDict = Compilation.GetSymbolDictionary(assign.Tree!);
        var variableSymbol = symbolDict
            .GetAllSymbols<IVariableSymbol>()
            .FirstOrDefault(v => v.Name == assign.Name);

        if (variableSymbol == null)
        {
            Error(assign, $"Undeclared variable '{assign.Name}' cannot be assigned.");
            return;
        }

        var exprType = ResolveExpressionType(assign.Expression, variableSymbol.Type);

        if (exprType != variableSymbol.Type && exprType != new TypeReference("unknown"))
            Error(assign, $"Type mismatch: Cannot assign expression of type '{exprType}' to variable '{variableSymbol.Name}' of type '{variableSymbol.Type}'.");
    }

    private TypeReference ResolveExpressionType(ExpressionNode expr, TypeReference? expectedType = null)
    {
        return expr switch
        {
            StringLiteralNode => new TypeReference("char", 1),
            CharacterLiteralNode => TypeReference.Char,
            IntegerLiteralNode => TypeReference.I32,
            BooleanLiteralNode => TypeReference.Bool,
            IdentifierNode => expectedType ?? new TypeReference("unknown"),
            _ => new TypeReference("unknown")
        };
    }
}