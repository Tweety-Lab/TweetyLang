using LLVMSharp;
using LLVMSharp.Interop;
using System.Collections.Generic;
using System.Reflection;
using TweetyLang.AST;
using TweetyLang.Parser.AST;

namespace TweetyLang.Emitter;

internal class IRBuilder
{
    private readonly LLVMModuleRef module;
    private readonly LLVMBuilderRef builder;
    private readonly LLVMContextRef context;

    public LLVMModuleRef Module => module;

    public IRBuilder(string moduleName = "TweetyModule")
    {
        context = LLVMContextRef.Create();
        module = LLVMModuleRef.CreateWithName(moduleName);
        builder = LLVMBuilderRef.Create(context);
    }

    public void EmitProgram(ProgramNode program)
    {
        foreach (var mod in program.Modules)
            EmitModule(mod);
    }

    private void EmitModule(ModuleNode module)
    {
        foreach (var fn in module.Functions)
            EmitFunction(fn);
    }

    private void EmitFunction(FunctionNode fn)
    {
        var retType = Mapping.MapType(fn.ReturnType);
        var fnType = LLVMTypeRef.CreateFunction(retType, System.Array.Empty<LLVMTypeRef>(), false);
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
