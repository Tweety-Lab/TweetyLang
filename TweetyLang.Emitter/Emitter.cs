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
}
