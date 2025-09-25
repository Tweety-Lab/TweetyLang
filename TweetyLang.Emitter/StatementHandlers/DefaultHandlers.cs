using TweetyLang.AST;

namespace TweetyLang.Emitter.StatementHandlers;

/// <summary>
/// The base class for all statement handlers.
/// </summary>
internal abstract class BaseStatementHandler
{
    public abstract bool CanHandle(StatementNode statement);
    public abstract void Handle(StatementNode statement, IRBuilder builder);
}

[StatementHandler]
internal class DeclarationHandler : BaseStatementHandler
{
    public override bool CanHandle(StatementNode statement) => statement is DeclarationNode;

    public override void Handle(StatementNode statement, IRBuilder builder)
    {
        if (statement is not DeclarationNode decl)
            return;

        // Allocate space for local variable
        var varType = Mapping.MapType(decl.Type);
        var alloca = builder.LLVMBuilder.BuildAlloca(varType, decl.Name);

        // Evaluate initializer expression
        var initValue = builder.EmitExpression(decl.Expression);

        // Store initializer
        builder.LLVMBuilder.BuildStore(initValue, alloca);

        // Track in locals (store both pointer and type)
        builder.FuncLocals[decl.Name] = (alloca, varType);
    }
}

[StatementHandler]
internal class ReturnHandler : BaseStatementHandler
{
    public override bool CanHandle(StatementNode statement) => statement is ReturnNode;

    public override void Handle(StatementNode statement, IRBuilder builder)
    {
        if (statement is not ReturnNode ret)
            return;

        if (ret.Expression == null)
        {
            builder.LLVMBuilder.BuildRetVoid();
            return;
        }

        var value = builder.EmitExpression(ret.Expression);

        builder.LLVMBuilder.BuildRet(value);
    }
}