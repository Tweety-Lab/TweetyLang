using TweetyLang.Parser.AST;

namespace TweetyLang;

internal class Program
{
    const string SOURCE = @"
module Program
{
    public i32 NumberFunc() 
    {
        return 32;
    }

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
        Console.WriteLine(Emitter.Emitter.EmitIR(tree));

    }
}
