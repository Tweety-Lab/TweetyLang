using Antlr4.Runtime.Misc;
using TweetyLang.AST;

namespace TweetyLang.Parser.AST;

/// <summary>
/// Visitor for building statement AST nodes.
/// </summary>
public partial class AstBuilder : TweetyLangBaseVisitor<AstNode>
{
    public override AstNode VisitStatement(TweetyLangParser.StatementContext context)
    {
        return Visit(context.raw_statement());
    }

    public override AstNode VisitDeclaration(TweetyLangParser.DeclarationContext context)
    {
        var declNode = new DeclarationNode
        {
            Name = context.identifier().GetText(),
            Type = BuildTypeReference(context.type())
        };
        declNode.Expression = declNode.AddChild(Visit(context.expression()) as ExpressionNode);
        return declNode;
    }

    public override AstNode VisitAssignment(TweetyLangParser.AssignmentContext context)
    {
        var assignNode = new AssignmentNode
        {
            Name = context.identifier().GetText()
        };
        assignNode.Expression = assignNode.AddChild(Visit(context.expression()) as ExpressionNode);
        return assignNode;
    }

    public override AstNode VisitReturn_statement(TweetyLangParser.Return_statementContext context)
    {
        var retNode = new ReturnNode();
        if (context.expression() != null)
            retNode.Expression = retNode.AddChild(Visit(context.expression()) as ExpressionNode);
        return retNode;
    }

    public override AstNode VisitExpression_statement(TweetyLangParser.Expression_statementContext context)
    {
        var expr = Visit(context.expression()) as ExpressionNode;

        if (expr is FunctionCallNode)
            return new ExpressionStatementNode { Expression = expr };

        return new ExpressionStatementNode { Expression = expr };
    }
}
