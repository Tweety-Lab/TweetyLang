using CommandLine;
using TweetyLang.Projects;

namespace TweetyLang.Verbs;

[Verb("new", HelpText = "Create a new project")]
internal class NewProject : BaseVerb
{
    [Option('n', "name", Required = true, HelpText = "Name of the project.")]
    public string Name { get; set; } = string.Empty;

    const string TEMPLATE_FILE = @"
import Methods;

module Methods 
{
    public i32 NumberFunc() 
    {
        return 32;
    }
}

module {PROJECT_NAME}
{
    public i32 main() 
    {
        i32 myVar = NumberFunc();
        return myVar;
    }
}
";

    public override void Run()
    {
        // Create a new directory for the project
        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), Name));

        // Create a new project file in the new directory
        Project project = new Project();
        project.OutputType = "ConsoleApp";
        Serialization.SaveProject(project, Path.Combine(Directory.GetCurrentDirectory(), Name, $"{Name}.toml"));

        // Create a template .tl file in the new directory
        string template = TEMPLATE_FILE.Replace("{PROJECT_NAME}", Name);
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), Name, $"{Name}.tl"), template);
    }
}
