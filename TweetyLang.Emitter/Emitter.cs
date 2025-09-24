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
        var irBuilder = new IRBuilder(tree.Root.Modules.First().Name); // This is moderately dumb
        irBuilder.EmitProgram(tree.Root);

        var module = irBuilder.Module;

        string irText = module.PrintToString();
        return irText;
    }
}
