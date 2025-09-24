using TweetyLang.AST;

namespace TweetyLang.Parser.Semantics;

public class SemanticAnalyzer
{
    private readonly List<string> errors = new List<string>();
    private readonly List<BaseSemanticRule> rules = new List<BaseSemanticRule>();

    /// <summary>
    /// A List of error messages.
    /// </summary>
    public IReadOnlyList<string> Errors => errors;

    public SemanticAnalyzer()
    {
        // Setup rules
        foreach (var type in typeof(BaseSemanticRule).Assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(BaseSemanticRule)) && type.IsDefined(typeof(SemanticAnalyzerAttribute), false))
                rules.Add((BaseSemanticRule)Activator.CreateInstance(type)!);
        }
    }

    public void Analyze(ProgramNode program)
    {
        try
        {
            foreach (var rule in rules)
                rule.AnalyzeProgram(program);
        } catch (SemanticException e)
        {
            errors.Add(e.Message);
        }

        foreach (var module in program.Modules)
            AnalyzeModule(module);
    }

    private void AnalyzeModule(ModuleNode module)
    {
        try
        {
            foreach (var rule in rules)
                rule.AnalyzeModule(module);
        } catch (SemanticException e) 
        {
            errors.Add(e.Message);
        }

        foreach (var fn in module.Functions)
            AnalyzeFunction(fn);
    }

    private void AnalyzeFunction(FunctionNode fn)
    {
        try
        {
            foreach (var rule in rules)
                rule.AnalyzeFunction(fn);
        } catch (SemanticException e)
        {
            errors.Add(e.Message);
        }
    }
}
