using CommandLine;
using LLVMSharp.Interop;
using TweetyLang.Compiler;
using TweetyLang.Compiler.Symbols;
using TweetyLang.Parser.AST;

namespace TweetyLang.Verbs;

[Verb("build", HelpText = "Builds the project in the current directory.")]
internal class BuildProject : BaseVerb
{
    public override void Run()
    {
        // Find the first .toml file in the current directory
        var projectFile = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.toml").FirstOrDefault();

        if (projectFile == null)
        {
            Console.Error.WriteLine("No project file found in the current directory.");
            return;
        }

        var project = Serialization.LoadProject(projectFile);

        // Recursively find all .tl files in the current directory
        var tlFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.tl", SearchOption.AllDirectories);

        List<TweetyLangSyntaxTree> syntaxTrees = new List<TweetyLangSyntaxTree>();

        foreach (var tlFile in tlFiles)
        {
            Console.WriteLine($"Compiling {Path.GetFileName(tlFile)}...");

            // Compile
            var tl = File.ReadAllText(tlFile);

            TweetyLangSyntaxTree syntaxTree = TweetyLangSyntaxTree.ParseText(tl);
            syntaxTrees.Add(syntaxTree);

            // Handle Syntax Errors
            if (syntaxTree.Errors.Count() > 0)
            {
                Console.WriteLine($"\nCompilation failed in {Path.GetFileName(tlFile)}!");

                foreach (var error in syntaxTree.Errors)
                    CompilerOutput.WriteError(error.Message, error.Line, error.Column);

                return;
            }
        }

        TweetyLangCompilation compilation = TweetyLangCompilation.Create(Path.GetFileNameWithoutExtension(projectFile), syntaxTrees);
        SymbolDictionary dict = compilation.GetSymbolDictionary(syntaxTrees[0]);
        IFunctionSymbol function = dict.GetDeclaredSymbol<IFunctionSymbol>(syntaxTrees[0].Root.Modules[0].Functions[0]);

        Console.WriteLine(function.Name);

        LLVMModuleRef module = Emitter.Emitter.EmitModule(compilation);
        Console.WriteLine(module.PrintToString());
    }
}
