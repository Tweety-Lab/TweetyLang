using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using TweetyLang.AST;
using TweetyLang.Parser.AST;

namespace TweetyLang;

internal class Program
{
    const string SOURCE = @"
import OtherModule;

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
        var tree = TweetyLangSyntaxTree.ParseText(SOURCE);
        var program = tree.Root;
        var method = program.Modules.FirstOrDefault(m => m.Name == "MyModule")
                        ?.Functions.FirstOrDefault(f => f.Name == "Main");

        if (method == null)
            return;

        // Log all errors
        if (tree.Errors.Count() > 0)
        {
            Console.WriteLine("Could not compile!");

            foreach (var error in tree.Errors)
                Console.WriteLine($"Error: {error.Message}");

            return;
        }

        Console.WriteLine($"Found main method '{method.Name}' with return type '{method.ReturnType}' and parameters '{string.Join(", ", method.Parameters.Select(a => a.Type))}'");
    }
}
