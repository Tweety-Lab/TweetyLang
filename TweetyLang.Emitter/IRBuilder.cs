using LLVMSharp;
using LLVMSharp.Interop;
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

    /// <summary>
    /// Store function pointer and its return type.
    /// </summary>
    public Dictionary<string, (LLVMValueRef Function, LLVMTypeRef FunctionType)> Funcs { get; set; } = new(); // This is stupid but until #229 gets fixed we have to do it... maybe? Prayers.

    /// <summary>
    /// The LLVM builder.
    /// </summary>
    public LLVMBuilderRef LLVMBuilder { get; }
    
    private LLVMModuleRef currentModule; // This is dumb
    private readonly LLVMContextRef context;
    private readonly List<BaseStatementHandler> handlers = new List<BaseStatementHandler>();

    private int nextStringID = 0;

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

            case CharacterLiteralNode charLit:
                return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, (ulong)charLit.Value, false);

            case StringLiteralNode strLit:
                // Generate a unique name for the string global
                var globalName = $"str{nextStringID++}";

                // Null-terminated string bytes
                var strValue = strLit.Value;
                byte[] strBytes = System.Text.Encoding.ASCII.GetBytes(strValue + "\0");

                var arrayType = LLVMTypeRef.CreateArray(LLVMTypeRef.Int8, (uint)strBytes.Length);
                var global = currentModule.AddGlobal(arrayType, globalName);

                // Create the initializer as a constant array of i8
                var constElements = strBytes.Select(b => LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, b, false)).ToArray();
                var initializer = LLVMValueRef.CreateConstArray(LLVMTypeRef.Int8, constElements);

                global.Initializer = initializer;
                global.Linkage = LLVMLinkage.LLVMPrivateLinkage;

                // Return a pointer to the first character (i8*)
                var zero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
                var ptr = LLVMBuilder.BuildInBoundsGEP2(arrayType, global, new LLVMValueRef[] { zero, zero }, "strptr");

                var i8PtrType = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
                return LLVMBuilder.BuildBitCast(ptr, i8PtrType, "strptr_cast");

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
                if (!Funcs.TryGetValue(call.Name, out var fnData))
                    throw new InvalidOperationException($"Unknown function {call.Name}");

                var args = call.Arguments?.Select(EmitExpression).ToArray() ?? Array.Empty<LLVMValueRef>();

                // Check if the function returns void
                if (fnData.FunctionType.ReturnType == LLVMTypeRef.Void)
                    return LLVMBuilder.BuildCall2(fnData.FunctionType, fnData.Function, args); // Functions that return void can't have a name
                else
                    return LLVMBuilder.BuildCall2(fnData.FunctionType, fnData.Function, args, "calltmp");


            default:
                throw new NotImplementedException($"Expression type {expr.GetType().Name} not implemented");
        }
    }

    [Obsolete("Use EmitModule instead (for now). See Emitter.Emitter for reference.")]
    public LLVMModuleRef EmitProgram(ProgramNode program)
    {
        var module = LLVMModuleRef.CreateWithName("Program");

        foreach (var mod in program.Modules)
            EmitModule(module, mod);

        return module;
    }

    public void EmitModule(LLVMModuleRef module, ModuleNode moduleNode)
    {
        foreach (var fn in moduleNode.Functions)
            EmitFunction(module, fn);

        foreach (var structDef in moduleNode.Structs)
            EmitStruct(module, structDef);
    }

    private void EmitFunction(LLVMModuleRef module, FunctionNode fn)
    {
        currentModule = module;
        FuncLocals.Clear();

        var retType = Mapping.MapType(fn.ReturnType);
        var paramsType = fn.Parameters.Select(p => Mapping.MapType(p.Type)).ToArray();
        var fnType = LLVMTypeRef.CreateFunction(retType, paramsType, false);
        var function = module.AddFunction(fn.Name, fnType);

        if (fn.Modifiers.HasFlag(Modifiers.Export) || fn.Modifiers.HasFlag(Modifiers.Extern))
            function.Linkage = LLVMLinkage.LLVMExternalLinkage;
        else
            function.Linkage = LLVMLinkage.LLVMInternalLinkage;

        Funcs[fn.Name] = (function, fnType);

        // If extern, we don't emit a body
        if (fn.Modifiers.HasFlag(Modifiers.Extern))
            return;

        // Entry block
        var entry = function.AppendBasicBlock("entry");
        LLVMBuilder.PositionAtEnd(entry);


        // Allocate space for parameters
        for (int i = 0; i < fn.Parameters.Count; i++)
        {
            var param = fn.Parameters[i];
            var llvmParam = function.GetParam((uint)i);
            llvmParam.Name = param.Name;

            var alloca = LLVMBuilder.BuildAlloca(Mapping.MapType(param.Type), param.Name);
            LLVMBuilder.BuildStore(llvmParam, alloca);

            FuncLocals[param.Name] = (alloca, Mapping.MapType(param.Type));
        }

        // Emit statements
        foreach (var stmt in fn.Body ?? new List<StatementNode>())
            EmitStatement(stmt);

        // Return
        if (fn.ReturnType == TypeReference.Void)
            LLVMBuilder.BuildRetVoid();
    }

    private void EmitStruct(LLVMModuleRef module, StructNode structNode)
    {
        var structType = context.CreateNamedStruct(structNode.Name);
        structType.StructSetBody(structNode.Fields.Select(f => Mapping.MapType(f.Type)).ToArray(), false);
    }

    private void EmitStatement(StatementNode stmt)
    {
        foreach (var handler in handlers)
            if (handler.CanHandle(stmt))
                handler.Handle(stmt, this);
    }
}
