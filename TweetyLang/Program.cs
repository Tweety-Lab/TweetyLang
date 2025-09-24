using LLVMSharp.Interop;
using TweetyLang.Parser.AST;

namespace TweetyLang;

internal class Program
{
    const string SOURCE = @"
module Methods 
{
    private i32 NumberFunc() 
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

        // Handle parsing errors
        if (tree.Errors.Count() > 0)
        {
            Console.WriteLine("Could not compile!");

            foreach (var error in tree.Errors)
                Console.WriteLine($"Error: {error.Message}");

            return;
        }

        // EMIT IR
        LLVMModuleRef module = Emitter.Emitter.EmitModule(tree);

        // PRINT IR
        Console.WriteLine(module.PrintToString());

    }
}
