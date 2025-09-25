
using TweetyLang.AST;

namespace TweetyLang.Parser.Semantics;

/// <summary>
/// Base class for semantic rules.
/// </summary>
internal abstract class BaseSemanticRule
{
    /// <summary> A list of exceptions that occured during semantic analysis in this rule. </summary>
    internal List<SemanticError> Exceptions { get; } = new();

    /// <summary> A list of warnings that occured during semantic analysis in this rule. </summary>
    internal List<SemanticWarning> Warnings { get; } = new();

    public virtual void AnalyzeFunction(FunctionNode func) { }
    public virtual void AnalyzeModule(ModuleNode module) { }
    public virtual void AnalyzeProgram(ProgramNode program) { }
    public virtual void AnalyzeStatement(StatementNode expr) { }
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
    public static Dictionary<string, (string ModuleName, string AccessModifier)> FunctionVisibility = new();

    public override void AnalyzeFunction(FunctionNode func)
    {
        // Find owning module
        var module = func.Ancestors().OfType<ModuleNode>().FirstOrDefault();
        if (module == null)
            throw new InvalidOperationException($"Function '{func.Name}' is not contained in a module.");

        FunctionVisibility[func.Name] = (module.Name, func.AccessModifier);
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

        var (declaringModule, access) = FunctionVisibility[call.Name];

        // Find the module of the current function call
        var currentModule = call.Ancestors().OfType<ModuleNode>().FirstOrDefault();
        if (currentModule == null)
        {
            Error(call, $"Function call '{call.Name}' is not inside any module.");
            return;
        }

        // Check module visibility: only allow calls to functions in the same module or imported modules
        var program = call.Ancestors().OfType<ProgramNode>().FirstOrDefault();
        bool isImported = program?.Imports.Any(i => i.ModuleName == declaringModule) ?? false;

        if (currentModule.Name != declaringModule && !isImported)
        {
            Error(call, $"Cannot call function '{call.Name}' from module '{declaringModule}' because it is not imported.");
        }

        // Check access modifier
        if (access != "public" && currentModule.Name != declaringModule)
        {
            Error(call, $"Tried to call private function '{call.Name}' from another module.");
        }
    }
}