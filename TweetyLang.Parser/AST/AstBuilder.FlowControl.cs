using TweetyLang.AST;

namespace TweetyLang.Parser.AST;

public partial class AstBuilder : TweetyLangBaseVisitor<AstNode>
{
    public override AstNode VisitIf_statement(TweetyLangParser.If_statementContext context)
    {
        var ifNode = new IfNode
        {
            SourceLine = context.Start.Line,
            SourceColumn = context.Start.Column
        };

        // condition
        ifNode.Condition = ifNode.AddChild(Visit(context.expression()) as ExpressionNode);

        // then-block
        foreach (var stmtCtx in context.statement_block().statement())
        {
            if (Visit(stmtCtx) is StatementNode stmt)
                ifNode.ThenBlock.Add(ifNode.AddChild(stmt));
        }

        // else-block
        if (context.else_block() != null)
        {
            ifNode.ElseBlock = new List<StatementNode>();
            foreach (var stmtCtx in context.else_block().statement_block().statement())
            {
                if (Visit(stmtCtx) is StatementNode stmt)
                    ifNode.ElseBlock.Add(ifNode.AddChild(stmt));
            }
        }

        return ifNode;
    }
}
