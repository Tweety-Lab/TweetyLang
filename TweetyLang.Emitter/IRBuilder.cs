using LLVMSharp;
using LLVMSharp.Interop;
using TweetyLang.AST;

namespace TweetyLang.Emitter;

internal class IRBuilder
{
    private readonly LLVMContextRef context;
    private readonly LLVMBuilderRef builder;

    public IRBuilder()
    {
        context = LLVMContextRef.Create();
        builder = LLVMBuilderRef.Create(context);
    }

    public IReadOnlyList<LLVMModuleRef> EmitProgram(ProgramNode program)
    {
        var modules = new List<LLVMModuleRef>();

        foreach (var mod in program.Modules)
            modules.Add(EmitModule(mod));

        return modules;
    }

    private LLVMModuleRef EmitModule(ModuleNode moduleNode)
    {
        var module = LLVMModuleRef.CreateWithName(moduleNode.Name);

        foreach (var fn in moduleNode.Functions)
            EmitFunction(module, fn);

        return module;
    }

    private void EmitFunction(LLVMModuleRef module, FunctionNode fn)
    {
        var retType = Mapping.MapType(fn.ReturnType);
        var paramsType = fn.Parameters.Select(p => Mapping.MapType(p.Type)).ToArray();
        var fnType = LLVMTypeRef.CreateFunction(retType, paramsType, false);
        var function = module.AddFunction(fn.Name, fnType);

        // Entry block
        var entry = function.AppendBasicBlock("entry");
        builder.PositionAtEnd(entry);

        // Emit statements
        foreach (var stmt in fn.Body)
            EmitStatement(stmt);

        // Return
        if (fn.ReturnType == "void")
            builder.BuildRetVoid();
    }

    private void EmitStatement(StatementNode stmt)
    {

    }
}
