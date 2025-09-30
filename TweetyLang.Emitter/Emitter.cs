using LLVMSharp.Interop;
using TweetyLang.Compiler;
using TweetyLang.Parser.AST;

namespace TweetyLang.Emitter;

public static class Emitter
{
    /// <summary>
    /// Emits a single linked LLVM module for the entire compilation.
    /// </summary>
    /// <param name="compilation">Compilation.</param>
    /// <returns>Linked LLVM module.</returns>
    public static LLVMModuleRef EmitModule(TweetyLangCompilation compilation)
    {
        var irBuilder = new IRBuilder();

        // Create one global LLVM module
        var mainModule = LLVMModuleRef.CreateWithName(compilation.AssemblyName);

        foreach (var tree in compilation.SyntaxTrees)
        {
            var program = tree.Root;

            foreach (var mod in program.Modules)
                irBuilder.EmitModule(mainModule, mod);
        }

        return mainModule;
    }
}
