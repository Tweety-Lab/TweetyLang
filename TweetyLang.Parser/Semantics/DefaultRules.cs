
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
    public static Dictionary<string, string> FunctionVisibility = new(); // <function name, function access>

    public override void AnalyzeFunction(FunctionNode func)
    {
        FunctionVisibility[func.Name] = func.AccessModifier;
    }

    public override void AnalyzeExpression(ExpressionNode expr)
    {
        if (expr is not FunctionCallNode call)
            return;

        // If function is not found
        if (!FunctionVisibility.ContainsKey(call.Name))
            Error(call, $"Tried to call unknown function '{call.Name}'.");

        // If function is found but private
        if (FunctionVisibility[call.Name] != "public")
            Error(call, $"Tried to call private function '{call.Name}'.");
    }
}