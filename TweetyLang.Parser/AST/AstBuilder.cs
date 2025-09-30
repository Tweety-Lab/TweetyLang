using TweetyLang.Parser;
using TweetyLang.AST;
using TweetyLang;

namespace TweetyLang.Parser.AST;

public partial class AstBuilder : TweetyLangBaseVisitor<AstNode>
{
    private readonly TweetyLangSyntaxTree syntaxTree;

    public AstBuilder(TweetyLangSyntaxTree syntaxTree) => this.syntaxTree = syntaxTree;

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
                program.Modules.Add(program.AddChild(module));

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

        foreach (var decl in context.module_block().definition())
        {
            AstNode node = Visit(decl);

            if (node is FunctionNode fn)
                module.Functions.Add(module.AddChild(fn));

            if (node is StructNode str)
                module.Structs.Add(module.AddChild(str));
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
            Modifiers = Modifiers.None,
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
                fn.Modifiers |= enumValue;
            }
        }

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

        if (context.statement_block() != null)
            fn.Body = BuildStatementBlock(context.statement_block(), fn);

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

    public List<StatementNode> BuildStatementBlock(TweetyLangParser.Statement_blockContext ctx, AstNode parent)
    {
        var statements = new List<StatementNode>();

        foreach (var stmtCtx in ctx.statement())
        {
            if (Visit(stmtCtx) is StatementNode stmt)
                statements.Add(parent.AddChild(stmt));
        }

        foreach (var compCtx in ctx.compound_statement())
        {
            if (Visit(compCtx) is StatementNode stmt)
                statements.Add(parent.AddChild(stmt));
        }

        return statements;
    }
}
