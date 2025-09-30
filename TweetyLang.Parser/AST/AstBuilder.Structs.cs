using TweetyLang.AST;

namespace TweetyLang.Parser.AST;

public partial class AstBuilder : TweetyLangBaseVisitor<AstNode>
{
    public override AstNode VisitStruct_definition(TweetyLangParser.Struct_definitionContext context)
    {
        var structNode = new StructNode
        {
            Name = context.identifier().GetText(),
            SourceLine = context.Start.Line,
            SourceColumn = context.Start.Column
        };

        // Handle modifiers
        if (context.modifier() != null)
        {
            foreach (var modCtx in context.modifier())
            {
                var modText = modCtx.GetText();
                var enumValue = (Modifiers)Enum.Parse(typeof(Modifiers), modText, true);
   
                structNode.Modifiers |= enumValue;
            }
        }

        // Visit object_block children
        foreach (var memberCtx in context.object_block().children)
        {
            switch (memberCtx)
            {
                case TweetyLangParser.Function_definitionContext fnCtx:
                    if (Visit(fnCtx) is FunctionNode fn)
                        structNode.Functions.Add(structNode.AddChild(fn));
                    break;

                case TweetyLangParser.Field_declarationContext fdCtx:
                    var field = new FieldDeclarationNode
                    {
                        Name = fdCtx.identifier().GetText(),
                        Type = BuildTypeReference(fdCtx.type()),
                        SourceLine = fdCtx.Start.Line,
                        SourceColumn = fdCtx.Start.Column
                    };

                    if (fdCtx.expression() != null)
                        field.Expression = Visit(fdCtx.expression()) as ExpressionNode;

                    structNode.Members.Add(structNode.AddChild(field));
                    break;
            }
        }

        return structNode;
    }
}
