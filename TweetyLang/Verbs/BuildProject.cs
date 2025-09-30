using CommandLine;
using LLVMSharp.Interop;
using TweetyLang.Compiler;
using TweetyLang.Linker;
using TweetyLang.Parser.AST;

namespace TweetyLang.Verbs;

[Verb("build", HelpText = "Builds the project in the current directory.")]
internal class BuildProject : BaseVerb
{
    public override void Run()
    {
        var module = BuildProjectFromDir(Directory.GetCurrentDirectory());
        Console.WriteLine(module.PrintToString());
    }

    public static LLVMModuleRef BuildProjectFromDir(string projectDir)
    {
        var projectFile = Directory.GetFiles(projectDir, "*.toml").FirstOrDefault();
        if (projectFile == null)
        {
            Console.Error.WriteLine($"No project file found in {projectDir}!");
            return default;
        }

        var project = Serialization.LoadProject(projectFile);

        // Collect all syntax trees from dependencies first
        var allSyntaxTrees = new List<TweetyLangSyntaxTree>();
        foreach (var dependency in project.Dependencies ?? new())
        {
            var depPath = Path.GetFullPath(Path.Combine(projectDir, dependency.Value.Path));
            var depTrees = CollectSyntaxTreesFromDir(depPath);
            if (depTrees == null) return default; // stop if dependency has errors
            allSyntaxTrees.AddRange(depTrees);
        }

        // Collect syntax trees from the current project
        var currentTrees = CollectSyntaxTreesFromDir(projectDir);
        if (currentTrees == null) return default;
        allSyntaxTrees.AddRange(currentTrees);

        var compilation = TweetyLangCompilation.Create(Path.GetFileNameWithoutExtension(projectFile), allSyntaxTrees);

        // Report warnings
        foreach (var warning in compilation.Warnings)
            CompilerOutput.WriteWarning(warning.Message, warning.Line, warning.Column);

        // Report errors
        if (compilation.Errors.Any())
        {
            Console.WriteLine("\nCould not compile project:");
            foreach (var error in compilation.Errors)
                CompilerOutput.WriteError(error.Message, error.Line, error.Column);

            return default;
        }

        LLVMModuleRef module = Emitter.Emitter.EmitModule(compilation, Enumerable.Empty<LLVMModuleRef>());

        // Write to .obj
        string outputDir = Path.Combine(projectDir, "bin");
        Directory.CreateDirectory(outputDir);

        string objDir = Path.Combine(outputDir, Path.ChangeExtension(Path.GetFileName(projectFile), ".o"));
        Linker.Linker.ModuleToObjectFile(module, objDir, LLVMTargetRef.DefaultTriple);

        AssemblyType assemblyType = AssemblyType.DynamicLibrary;

        // Write to an assembly depending on project type
        if (project.OutputType == "ConsoleApp")
            assemblyType = AssemblyType.Application;
        else if (project.OutputType == "DynamicLibrary")
            assemblyType = AssemblyType.DynamicLibrary;
        else if (project.OutputType == "StaticLibrary")
            assemblyType = AssemblyType.StaticLibrary;

        string assemblyDir = Path.Combine(outputDir, Path.GetFileName(projectFile));
        Linker.Linker.ObjectFilesToAssembly(new[] { objDir }, assemblyDir, assemblyType);

        return module;
    }

    /// <summary>
    /// Parses all .tl files in a directory into syntax trees. Returns null if there are syntax errors.
    /// </summary>
    private static List<TweetyLangSyntaxTree>? CollectSyntaxTreesFromDir(string dir)
    {
        var syntaxTrees = new List<TweetyLangSyntaxTree>();
        var tlFiles = Directory.GetFiles(dir, "*.tl", SearchOption.AllDirectories);

        foreach (var tlFile in tlFiles)
        {
            Console.WriteLine($"Parsing {Path.GetFileName(tlFile)}...");
            var source = File.ReadAllText(tlFile);
            var tree = TweetyLangSyntaxTree.ParseText(source);

            if (tree.Errors.Any())
            {
                Console.WriteLine($"\nSyntax errors in {Path.GetFileName(tlFile)}:");
                foreach (var error in tree.Errors)
                    CompilerOutput.WriteError(error.Message, error.Line, error.Column);

                return null;
            }

            syntaxTrees.Add(tree);
        }

        return syntaxTrees;
    }
}
