using LLVMSharp.Interop;
using TweetyLang.Parser.AST;

namespace TweetyLang;

internal class Program
{
    const string SOURCE = @"
import Methods;

module Methods 
{
    public i32 NumberFunc() 
    {
        return 32;
    }
}

module Program
{
    public i32 main() 
    {
        return NumberFunc();
    }
}
";
    static void Main(string[] args)
    {
        // PARSE
        var tree = TweetyLangSyntaxTree.ParseText(SOURCE);

        // Handle parsing warnings
        if (tree.Warnings.Count() > 0)
        {
            foreach (var warning in tree.Warnings)
                CompilerOutput.WriteWarning(warning.Message, warning.Line, warning.Column);
        }

        // Handle parsing errors
        if (tree.Errors.Count() > 0)
        {
            Console.WriteLine("\nCompilation failed!");

            foreach (var error in tree.Errors)
                CompilerOutput.WriteError(error.Message, error.Line, error.Column);

            return;
        }

        LLVMModuleRef module = Emitter.Emitter.EmitModule(tree);

        // PRINT IR
        Console.WriteLine(module.PrintToString());
    }
}
