using TweetyLang.Parser.AST;

namespace TweetyLang;

internal class Program
{
    const string SOURCE = @"
module MyModule 
{
    public bool Main(i32 a, i32 b) 
    {
        bool x = true;
        return x;
    }
}
";
    static void Main(string[] args)
    {
        // PARSE
        var tree = TweetyLangSyntaxTree.ParseText(SOURCE);

        // Handle errors
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
