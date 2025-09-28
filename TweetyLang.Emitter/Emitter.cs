using LLVMSharp.Interop;
using TweetyLang.Compiler;
using TweetyLang.Parser.AST;

namespace TweetyLang.Emitter;

public static class Emitter
{
    /// <summary>
    /// Converts a TweetyLangCompilation into LLVM IR.
    /// </summary>
    /// <param name="compilation">Compilation.</param>
    /// <returns>LLVM IR as text.</returns>
    public static string EmitIR(TweetyLangCompilation compilation)
    {
        var irBuilder = new IRBuilder();
        var allModules = new List<LLVMModuleRef>();

        // Emit each tree in the compilation
        foreach (var tree in compilation.SyntaxTrees)
        {
            var modules = irBuilder.EmitProgram(tree.Root);
            allModules.AddRange(modules);
        }

        // Concatenate IR from all modules
        var irText = string.Join("\n", allModules.Select(m => m.PrintToString()));
        return irText;
    }


    /// <summary>
    /// Emits a single linked LLVM module for the entire compilation.
    /// </summary>
    /// <param name="compilation">Compilation.</param>
    /// <returns>Linked LLVM module.</returns>
    public static LLVMModuleRef EmitModule(TweetyLangCompilation compilation)
    {
        var irBuilder = new IRBuilder();
        var allModules = new List<LLVMModuleRef>();

        // Emit each tree
        foreach (var tree in compilation.SyntaxTrees)
        {
            var modules = irBuilder.EmitProgram(tree.Root);
            allModules.AddRange(modules);
        }

        if (allModules.Count == 0)
            throw new InvalidOperationException("Compilation contains no modules to emit.");

        // Link all modules together
        var mainModule = allModules[0];
        for (int i = 1; i < allModules.Count; i++)
        {
            var srcModule = allModules[i];
            unsafe
            {
                int result = LLVM.LinkModules2(mainModule, srcModule);
                if (result != 0)
                    throw new InvalidOperationException($"Failed to link module {i}.");
            }
        }

        return mainModule;
    }
}
