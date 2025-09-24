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
        return new DeclarationNode
        {
            Name = context.identifier().GetText(),
            Type = BuildTypeReference(context.type()),
            Expression = Visit(context.expression()) as ExpressionNode
        };
    }

    public override AstNode VisitReturn_statement(TweetyLangParser.Return_statementContext context)
    {
        return new ReturnNode
        {
            Expression = context.expression() != null
                ? Visit(context.expression()) as ExpressionNode
                : null
        };
    }
}
