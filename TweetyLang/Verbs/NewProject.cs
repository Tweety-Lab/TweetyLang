using CommandLine;

namespace TweetyLang.Verbs;

[Verb("new", HelpText = "Create a new project")]
internal class NewProject : BaseVerb
{
    [Option('n', "name", Required = true, HelpText = "Name of the project.")]
    public string Name { get; set; } = string.Empty;

    public override void Run()
    {
        throw new NotImplementedException();
    }
}
