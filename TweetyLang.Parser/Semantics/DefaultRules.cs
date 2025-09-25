
using TweetyLang.AST;

namespace TweetyLang.Parser.Semantics;

/// <summary>
/// Base class for semantic rules.
/// </summary>
internal abstract class BaseSemanticRule
{
    public virtual void AnalyzeFunction(FunctionNode func) { }
    public virtual void AnalyzeModule(ModuleNode module) { }
    public virtual void AnalyzeProgram(ProgramNode program) { }
    public virtual void AnalyzeStatement(StatementNode expr) { }
    public virtual void AnalyzeExpression(ExpressionNode expr) { }
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
                throw new SemanticException(func.Line, func.Column, $"Duplicate parameter '{p.Name}' in function '{func.Name}'.");
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
                throw new SemanticException(module.Line, module.Column, $"Duplicate function '{fn.Name}' in module '{module.Name}'.");
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
            throw new SemanticException(expr.Line, expr.Column, $"Tried to call unknown function '{call.Name}'.");

        // If function is found but private
        if (FunctionVisibility[call.Name] != "public")
            throw new SemanticException(expr.Line, expr.Column, $"Tried to call private function '{call.Name}'.");
    }
}