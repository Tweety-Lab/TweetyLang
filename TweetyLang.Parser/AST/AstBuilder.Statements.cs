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

    public override AstNode VisitReturn_statement(TweetyLangParser.Return_statementContext context)
    {
        var retNode = new ReturnNode();
        if (context.expression() != null)
            retNode.Expression = retNode.AddChild(Visit(context.expression()) as ExpressionNode);
        return retNode;
    }
}
