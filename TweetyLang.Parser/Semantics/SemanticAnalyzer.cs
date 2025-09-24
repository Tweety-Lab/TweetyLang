using TweetyLang.AST;

namespace TweetyLang.Parser.Semantics;

public class SemanticAnalyzer
{
    private readonly List<SemanticException> errors = new List<SemanticException>();
    private readonly List<BaseSemanticRule> rules = new List<BaseSemanticRule>();

    /// <summary>
    /// A List of error messages.
    /// </summary>
    public IReadOnlyList<SemanticException> Errors => errors;

    public SemanticAnalyzer()
    {
        // Setup rules
        foreach (var type in typeof(BaseSemanticRule).Assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(BaseSemanticRule)) && type.IsDefined(typeof(SemanticAnalyzerAttribute), false))
                rules.Add((BaseSemanticRule)Activator.CreateInstance(type)!);
        }
    }

    public void Analyze(ProgramNode program)
    {
        try
        {
            foreach (var rule in rules)
                rule.AnalyzeProgram(program);
        } catch (SemanticException e)
        {
            errors.Add(e);
        }

        foreach (var module in program.Modules)
            AnalyzeModule(module);
    }

    private void AnalyzeModule(ModuleNode module)
    {
        try
        {
            foreach (var rule in rules)
                rule.AnalyzeModule(module);
        } catch (SemanticException e) 
        {
            errors.Add(e);
        }

        foreach (var fn in module.Functions)
            AnalyzeFunction(fn);
    }

    private void AnalyzeFunction(FunctionNode fn)
    {
        try
        {
            foreach (var rule in rules)
                rule.AnalyzeFunction(fn);
        } catch (SemanticException e)
        {
            errors.Add(e);
        }

        foreach (var stmt in fn.Body)
            AnalyzeStatement(stmt);
    }

    private void AnalyzeStatement(StatementNode stmt)
    {
        try
        {
            foreach (var rule in rules)
                rule.AnalyzeStatement(stmt);
        } catch (SemanticException e)
        {
            errors.Add(e);
        }

        // Recursively analyze expressions inside statements
        switch (stmt)
        {
            case DeclarationNode decl:
                AnalyzeExpression(decl.Expression);
                break;

            case ReturnNode ret:
                AnalyzeExpression(ret.Expression);
                break;
        }
    }

    private void AnalyzeExpression(ExpressionNode expr)
    {
        if (expr == null)
            return;

        try
        {
            foreach (var rule in rules)
                rule.AnalyzeExpression(expr);
        }
        catch (SemanticException e)
        {
            errors.Add(e);
        }

        // Recursively analyze child expressions
        switch (expr)
        {
            case BinaryExpressionNode bin:
                AnalyzeExpression(bin.Left);
                AnalyzeExpression(bin.Right);
                break;

            case FunctionCallNode call:
                foreach (var arg in call.Arguments)
                    AnalyzeExpression(arg);
                break;

            case IdentifierNode:
            case IntegerLiteralNode:
            case BooleanLiteralNode:
                break; // leaf nodes, nothing to recurse into

            default:
                throw new NotImplementedException($"Expression type {expr.GetType().Name} not implemented in semantic analysis");
        }
    }
}
