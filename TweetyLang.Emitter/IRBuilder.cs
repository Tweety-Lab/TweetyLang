using LLVMSharp;
using LLVMSharp.Interop;
using System;
using System.Reflection;
using TweetyLang.AST;
using TweetyLang.Emitter.StatementHandlers;

namespace TweetyLang.Emitter;

internal class IRBuilder
{
    /// <summary>
    /// The local variables of the current function. Stores Pointer and Type.
    /// </summary>
    public Dictionary<string, (LLVMValueRef Pointer, LLVMTypeRef Type)> FuncLocals { get; set; } = new();

    private Dictionary<LLVMModuleRef, Dictionary<string, (LLVMValueRef Function, LLVMTypeRef Type)>> moduleFuncs = new();
    private Dictionary<string, (LLVMValueRef Function, LLVMTypeRef Type)> publicFuncs = new();

    /// <summary>
    /// The LLVM builder.
    /// </summary>
    public LLVMBuilderRef LLVMBuilder { get; }
    
    private LLVMModuleRef currentModule; // This is dumb
    private readonly LLVMContextRef context;
    private readonly List<BaseStatementHandler> handlers = new List<BaseStatementHandler>();

    public IRBuilder()
    {
        context = LLVMContextRef.Create();
        LLVMBuilder = LLVMBuilderRef.Create(context);

        // Reflect all classes with StatementHandlerAttribute and add them to handlers
        foreach (var type in typeof(BaseStatementHandler).Assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<StatementHandlerAttribute>();
            if (attr != null)
                handlers.Add((BaseStatementHandler)Activator.CreateInstance(type)!);
        }
    }

    public LLVMValueRef EmitExpression(ExpressionNode expr)
    {
        switch (expr)
        {
            case IntegerLiteralNode intLit:
                return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)intLit.Value, false);

            case BooleanLiteralNode boolLit:
                return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, boolLit.Value ? 1UL : 0UL, false);

            case IdentifierNode id:
                if (!FuncLocals.TryGetValue(id.Name, out var value))
                    throw new InvalidOperationException($"Unknown variable {id.Name}");

                var (pointer, type) = value;
                return LLVMBuilder.BuildLoad2(type, pointer, id.Name);

            case BinaryExpressionNode bin:
                var left = EmitExpression(bin.Left);
                var right = EmitExpression(bin.Right);
                return bin.Operator switch
                {
                    "+" => LLVMBuilder.BuildAdd(left, right, "addtmp"),
                    "-" => LLVMBuilder.BuildSub(left, right, "subtmp"),
                    "*" => LLVMBuilder.BuildMul(left, right, "multmp"),
                    "/" => LLVMBuilder.BuildSDiv(left, right, "divtmp"),
                    _ => throw new NotImplementedException($"Unknown operator {bin.Operator}")
                };
            case FunctionCallNode call:
                if (!moduleFuncs[currentModule].TryGetValue(call.Name, out var fnData))
                {
                    // If not found locally, check public functions
                    if (!publicFuncs.TryGetValue(call.Name, out fnData))
                        throw new InvalidOperationException($"Cannot access function '{call.Name}' from this module");
                }

                var args = call.Arguments?.Select(EmitExpression).ToArray() ?? Array.Empty<LLVMValueRef>();
                return LLVMBuilder.BuildCall2(fnData.Type, fnData.Function, args, "calltmp");


            default:
                throw new NotImplementedException($"Expression type {expr.GetType().Name} not implemented");
        }
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

        moduleFuncs[module] = new Dictionary<string, (LLVMValueRef, LLVMTypeRef)>();

        foreach (var fn in moduleNode.Functions)
            EmitFunction(module, fn);

        return module;
    }

    private void EmitFunction(LLVMModuleRef module, FunctionNode fn)
    {
        currentModule = module;
        FuncLocals.Clear();

        var retType = Mapping.MapType(fn.ReturnType);
        var paramsType = fn.Parameters.Select(p => Mapping.MapType(p.Type)).ToArray();
        var fnType = LLVMTypeRef.CreateFunction(retType, paramsType, false);
        var function = module.AddFunction(fn.Name, fnType);

        if (fn.AccessModifier == "public")
        {
            function.Linkage = LLVMLinkage.LLVMExternalLinkage;
            publicFuncs[fn.Name] = (function, fnType);
        } 
        else if (fn.AccessModifier == "private")
        {
            function.Linkage = LLVMLinkage.LLVMPrivateLinkage;
        }

            // Entry block
            var entry = function.AppendBasicBlock("entry");
        LLVMBuilder.PositionAtEnd(entry);

        // Emit statements
        foreach (var stmt in fn.Body)
            EmitStatement(stmt);

        // Return
        if (fn.ReturnType == TypeReference.Void)
            LLVMBuilder.BuildRetVoid();
    }

    private void EmitStatement(StatementNode stmt)
    {
        foreach (var handler in handlers)
            if (handler.CanHandle(stmt))
                handler.Handle(stmt, this);
    }
}
