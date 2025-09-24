
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
                throw new SemanticException($"Duplicate parameter '{p.Name}' in function '{func.Name}'.");
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
                throw new SemanticException($"Duplicate function '{fn.Name}' in module '{module.Name}'.");
        }
    }
}

[SemanticAnalyzer]
internal class FunctionCallVisibilityRule : BaseSemanticRule
{
    public static Dictionary<string, string> FunctionVisibility = new(); // <function name, function access>

    public override void AnalyzeFunction(FunctionNode func)
    {
        FunctionVisibility[func.AccessModifier] = func.Name;
    }

    public override void AnalyzeExpression(ExpressionNode expr)
    {
        if (expr is not FunctionCallNode call)
            return;

        // If function name is not visible, throw exception
        if (!FunctionVisibility.ContainsKey(call.Name) || call.Name != FunctionVisibility[call.Name])
            throw new SemanticException($"Tried to call function '{call.Name}' but it is not visible! Did you mean to make it public?");
    }
}
