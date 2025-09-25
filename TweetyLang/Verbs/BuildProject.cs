using CommandLine;
using LLVMSharp.Interop;
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

        foreach (var tlFile in tlFiles)
        {
            Console.WriteLine($"Compiling {Path.GetFileName(tlFile)}...");

            // Compile
            var tl = File.ReadAllText(tlFile);

            TweetyLangSyntaxTree syntaxTree = TweetyLangSyntaxTree.ParseText(tl);

            // Handle parsing warnings
            if (syntaxTree.Warnings.Count() > 0)
            {
                foreach (var warning in syntaxTree.Warnings)
                    CompilerOutput.WriteWarning(warning.Message, warning.Line, warning.Column);
            }

            // Handle parsing errors
            if (syntaxTree.Errors.Count() > 0)
            {
                Console.WriteLine("\nCompilation failed!");

                foreach (var error in syntaxTree.Errors)
                    CompilerOutput.WriteError(error.Message, error.Line, error.Column);

                return;
            }

            LLVMModuleRef module = Emitter.Emitter.EmitModule(syntaxTree);

            // PRINT IR
            Console.WriteLine("\n" + module.PrintToString());
        }
    }
}
