using TweetyLang.Parser;
using TweetyLang.AST;
using TweetyLang;

namespace TweetyLang.Parser.AST;

public partial class AstBuilder : TweetyLangBaseVisitor<AstNode>
{
    public override AstNode VisitProgram(TweetyLangParser.ProgramContext context)
    {
        var program = new ProgramNode();
        foreach (var decl in context.top_level_declaration())
        {
            var node = Visit(decl) as ModuleNode;
            if (node != null)
                program.Modules.Add(node);
        }
        return program;
    }

    public override AstNode VisitModule_definition(TweetyLangParser.Module_definitionContext context)
    {
        var module = new ModuleNode
        {
            Name = context.module_name().GetText()
        };

        foreach (var decl in context.module_body().top_level_declaration())
        {
            var fn = Visit(decl) as FunctionNode;
            if (fn != null)
                module.Functions.Add(fn);
        }

        return module;
    }

    public override AstNode VisitFunction_definition(TweetyLangParser.Function_definitionContext context)
    {
        var fn = new FunctionNode
        {
            Name = context.identifier().GetText(),
            ReturnType = context.type()?.GetText() ?? "void",
            AccessModifier = context.access_modifier().GetText()
        };

        if (context.parameters() != null)
        {
            foreach (var p in context.parameters().parameter())
            {
                fn.Parameters.Add(new ParameterNode
                {
                    Name = p.identifier().GetText(),
                    Type = p.type().GetText()
                });
            }
        }

        if (context.function_body() != null)
        {
            foreach (var stmtCtx in context.function_body().statement())
            {
                var stmt = Visit(stmtCtx) as StatementNode;
                if (stmt != null)
                    fn.Body.Add(stmt);
            }
        }

        return fn;
    }
}
