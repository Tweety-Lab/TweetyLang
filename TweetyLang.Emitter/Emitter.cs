using LLVMSharp.Interop;
using TweetyLang.Parser.AST;

namespace TweetyLang.Emitter;

public static class Emitter
{
    /// <summary>
    /// Converts a TweetyLangSyntaxTree into LLVM IR.
    /// </summary>
    /// <param name="tree">Syntax tree.</param>
    /// <returns>LLVM IR.</returns>
    public static string EmitIR(TweetyLangSyntaxTree tree)
    {
        var irBuilder = new IRBuilder();
        var modules = irBuilder.EmitProgram(tree.Root);

        // Concatenate IR from all modules
        var irText = string.Join("\n", modules.Select(m => m.PrintToString()));

        return irText;
    }


    public static LLVMModuleRef EmitModule(TweetyLangSyntaxTree tree)
    {
        var irBuilder = new IRBuilder();
        var modules = irBuilder.EmitProgram(tree.Root);

        // Link all modules with LLVM linker
        LLVMModuleRef module;
        if (modules.Count == 1)
        {
            module = modules[0];
        }
        else
        {
            // Link all modules into the first module
            module = modules[0];
            for (int i = 1; i < modules.Count; i++)
            {
                var src = modules[i];

                unsafe
                {
                    int result = LLVM.LinkModules2(module, src);
                    if (result != 0)
                        throw new InvalidOperationException($"Failed to link module {i}.");
                }
            }
        }

        // Return singular module
        return module;
    }
}
