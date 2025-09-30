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
    public static LLVMModuleRef EmitModule(TweetyLangCompilation compilation, IEnumerable<LLVMModuleRef>? dependencies = null)
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


        // Link all dependencies
        foreach (var dep in dependencies ?? Enumerable.Empty<LLVMModuleRef>())
        {
            unsafe
            {
                int result = LLVM.LinkModules2(mainModule, dep);
                if (result != 0)
                    throw new InvalidOperationException("Failed to link dependency module.");
            }
        }

        return mainModule;
    }
}
