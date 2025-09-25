using TweetyLang.AST;

namespace TweetyLang.Parser.Semantics;

public class SemanticAnalyzer
{
    private readonly List<SemanticError> errors = new List<SemanticError>();
    private readonly List<SemanticWarning> warnings = new();
    private readonly List<BaseSemanticRule> rules = new List<BaseSemanticRule>();

    /// <summary>
    /// A List of error messages.
    /// </summary>
    public IReadOnlyList<SemanticError> Errors => errors;

    /// <summary>
    /// A List of warning messages.
    /// </summary>
    public IReadOnlyList<SemanticWarning> Warnings => warnings;

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
        foreach (var rule in rules)
        {
            rule.AnalyzeProgram(program);
            errors.AddRange(rule.Exceptions);
            warnings.AddRange(rule.Warnings);

            // Reset exceptions and warnings
            rule.Exceptions.Clear();
            rule.Warnings.Clear();
        }

        foreach (var module in program.Modules)
            AnalyzeModule(module);
    }

    private void AnalyzeModule(ModuleNode module)
    {
        foreach (var rule in rules)
        {
            rule.AnalyzeModule(module);
            errors.AddRange(rule.Exceptions);
            warnings.AddRange(rule.Warnings);

            // Reset exceptions and warnings
            rule.Exceptions.Clear();
            rule.Warnings.Clear();
        }

        foreach (var fn in module.Functions)
            AnalyzeFunction(fn);
    }

    private void AnalyzeFunction(FunctionNode fn)
    {
        foreach (var rule in rules)
        {
            rule.AnalyzeFunction(fn);
            errors.AddRange(rule.Exceptions);
            warnings.AddRange(rule.Warnings);

            // Reset exceptions and warnings
            rule.Exceptions.Clear();
            rule.Warnings.Clear();
        }

        foreach (var stmt in fn.Body)
            AnalyzeStatement(stmt);
    }

    private void AnalyzeStatement(StatementNode stmt)
    {
        foreach (var rule in rules)
        {
            rule.AnalyzeStatement(stmt);
            errors.AddRange(rule.Exceptions);
            warnings.AddRange(rule.Warnings);

            // Reset exceptions and warnings
            rule.Exceptions.Clear();
            rule.Warnings.Clear();
        }

        // Analyze expressions inside statements
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

        foreach (var rule in rules)
        {
            rule.AnalyzeExpression(expr);
            errors.AddRange(rule.Exceptions);
            warnings.AddRange(rule.Warnings);

            // Reset exceptions and warnings
            rule.Exceptions.Clear();
            rule.Warnings.Clear();
        }

        // Analyze child expressions
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
