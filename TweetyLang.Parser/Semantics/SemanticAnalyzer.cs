using TweetyLang.AST;

namespace TweetyLang.Parser.Semantics;

public class SemanticAnalyzer
{
    private readonly List<string> errors = new List<string>();
    private readonly List<BaseSemanticRule> rules = new List<BaseSemanticRule>();

    public SemanticAnalyzer()
    {
        // Setup rules
        foreach (var type in typeof(BaseSemanticRule).Assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(BaseSemanticRule)) && type.IsDefined(typeof(SemanticAnalyzerAttribute), false))
                rules.Add((BaseSemanticRule)Activator.CreateInstance(type)!);
        }
    }

    /// <summary>
    /// A List of error messages.
    /// </summary>
    public IReadOnlyList<string> Errors => errors;

    public void Analyze(ProgramNode program)
    {
        foreach (var rule in rules)
            rule.AnalyzeProgram(program);

        foreach (var module in program.Modules)
            AnalyzeModule(module);
    }

    private void AnalyzeModule(ModuleNode module)
    {
        foreach (var rule in rules)
            rule.AnalyzeModule(module);

        foreach (var fn in module.Functions)
            AnalyzeFunction(fn);
    }

    private void AnalyzeFunction(FunctionNode fn)
    {
        foreach (var rule in rules)
            rule.AnalyzeFunction(fn);
    }
}
