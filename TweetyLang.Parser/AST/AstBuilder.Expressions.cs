using TweetyLang.AST;

namespace TweetyLang.Parser.AST;

/// <summary>
/// Visitor for building expression AST nodes.
/// </summary>
public partial class AstBuilder : TweetyLangBaseVisitor<AstNode>
{
    public override AstNode VisitIdentifier(TweetyLangParser.IdentifierContext context)
    {
        return new IdentifierNode
        {
            Name = context.GetText(),
            SourceLine = context.Start.Line,
            SourceColumn = context.Start.Column
        };
    }

    public override AstNode VisitBoolean_literal(TweetyLangParser.Boolean_literalContext context)
    {
        return new BooleanLiteralNode
        {
            Value = context.GetText() == "true",
            SourceLine = context.Start.Line,
            SourceColumn = context.Start.Column
        };
    }

    public override AstNode VisitFactor(TweetyLangParser.FactorContext context)
    {
        if (context.identifier() != null)
            return Visit(context.identifier());

        if (context.boolean_literal() != null)
            return Visit(context.boolean_literal());

        if (context.NUMBER() != null)
            return new IntegerLiteralNode { Value = int.Parse(context.NUMBER().GetText()) };

        if (context.function_call() != null)
            return Visit(context.function_call());

        if (context.expression() != null) // parentheses
            return Visit(context.expression());

        throw new NotImplementedException("Unknown factor type");
    }

    public override AstNode VisitTerm(TweetyLangParser.TermContext context)
    {
        var node = Visit(context.factor(0)) as ExpressionNode;

        for (int i = 1; i < context.factor().Length; i++)
        {
            var op = context.GetChild(2 * i - 1).GetText(); // '*' or '/'
            var right = Visit(context.factor(i)) as ExpressionNode;

            node = new BinaryExpressionNode
            {
                Operator = op,
                Left = node,
                Right = right
            };
        }

        return node;
    }

    public override AstNode VisitFunction_call(TweetyLangParser.Function_callContext context)
    {
        var call = new FunctionCallNode
        {
            Name = context.identifier().GetText(),
            SourceLine = context.Start.Line,
            SourceColumn = context.Start.Column
        };

        if (context.arguments() != null)
        {
            foreach (var argCtx in context.arguments().expression())
            {
                var argNode = Visit(argCtx) as ExpressionNode;
                if (argNode != null)
                    call.Arguments.Add(argNode);
            }
        }

        return call;
    }

    public override AstNode VisitExpression(TweetyLangParser.ExpressionContext context)
    {
        var node = Visit(context.term(0)) as ExpressionNode;

        for (int i = 1; i < context.term().Length; i++)
        {
            var op = context.GetChild(2 * i - 1).GetText(); // '+' or '-'
            var right = Visit(context.term(i)) as ExpressionNode;

            node = new BinaryExpressionNode
            {
                Operator = op,
                Left = node,
                Right = right,
                SourceLine = context.Start.Line,
                SourceColumn = context.Start.Column
            };
        }

        return node;
    }
}
