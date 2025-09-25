using TweetyLang.Parser;
using TweetyLang.AST;
using TweetyLang;

namespace TweetyLang.Parser.AST;

public partial class AstBuilder : TweetyLangBaseVisitor<AstNode>
{
    public override AstNode VisitProgram(TweetyLangParser.ProgramContext context)
    {
        var program = new ProgramNode
        {
            SourceLine = context.Start.Line,
            SourceColumn = context.Start.Column
        };

        foreach (var decl in context.top_level_declaration())
        {
            var node = Visit(decl);
            if (node is ModuleNode module)
            {
                program.Modules.Add(program.AddChild(module));
                module.Functions = module.AddChildren(module.Functions).ToList();
            }

            if (node is ImportNode import)
                program.Imports.Add(program.AddChild(import));
        }

        return program;
    }

    public override AstNode VisitModule_definition(TweetyLangParser.Module_definitionContext context)
    {
        var module = new ModuleNode
        {
            Name = context.module_name().GetText(),
            SourceLine = context.Start.Line,
            SourceColumn = context.Start.Column
        };

        foreach (var decl in context.module_body().top_level_declaration())
        {
            if (Visit(decl) is FunctionNode fn)
                module.Functions.Add(module.AddChild(fn));
        }

        return module;
    }

    public override AstNode VisitFunction_definition(TweetyLangParser.Function_definitionContext context)
    {
        var returnTypeCtx = context.type();
        var returnType = returnTypeCtx != null
            ? BuildTypeReference(returnTypeCtx)
            : new TypeReference("void");

        var fn = new FunctionNode
        {
            Name = context.identifier().GetText(),
            ReturnType = returnType,
            AccessModifier = context.access_modifier().GetText(),
            SourceLine = context.Start.Line,
            SourceColumn = context.Start.Column
        };

        if (context.parameters() != null)
        {
            foreach (var p in context.parameters().parameter())
            {
                var paramNode = new ParameterNode
                {
                    Name = p.identifier().GetText(),
                    Type = BuildTypeReference(p.type())
                };
                fn.Parameters.Add(fn.AddChild(paramNode));
            }
        }

        if (context.function_body() != null)
        {
            foreach (var stmtCtx in context.function_body().statement())
            {
                if (Visit(stmtCtx) is StatementNode stmt)
                    fn.Body.Add(fn.AddChild(stmt));
            }
        }

        return fn;
    }

    public override AstNode VisitImport_statement(TweetyLangParser.Import_statementContext context)
    {
        return new ImportNode
        {
            ModuleName = context.module_name().GetText(),
            SourceLine = context.Start.Line,
            SourceColumn = context.Start.Column
        };
    }

    private TypeReference BuildTypeReference(TweetyLangParser.TypeContext ctx)
    {
        var baseType = ctx.raw_type().GetText();
        int pointerLevel = ctx.pointer_suffix()?.GetText().Count(c => c == '*') ?? 0;
        return new TypeReference(baseType, pointerLevel);
    }
}
