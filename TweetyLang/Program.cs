using CommandLine;
using TweetyLang.Verbs;

namespace TweetyLang;

internal class Program
{
    static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<NewProject, BuildProject>(args)
            .WithParsed<BaseVerb>(verb => verb.Run())
            .WithNotParsed(errors =>
            {
                foreach (var error in errors)
                    Console.WriteLine($"Argument error: {error}");
            });
    }
}
