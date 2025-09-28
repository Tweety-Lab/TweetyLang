using TweetyLang.Compiler.Symbols;
using TweetyLang.Parser.AST;
using TweetyLang.Parser.Semantics;

namespace TweetyLang.Compiler;

/// <summary>
/// Represents a compilation of TweetyLang code, typically a assembly.
/// </summary>
public class TweetyLangCompilation
{
    private readonly List<TweetyLangSyntaxTree> syntaxTrees;

    private readonly Dictionary<TweetyLangSyntaxTree, SymbolDictionary> symbolDictionaries = new();

    /// <summary> The syntax trees in the compilation. </summary>
    public IEnumerable<TweetyLangSyntaxTree> SyntaxTrees => syntaxTrees;

    /// <summary> The semantic errors in the compilation. </summary>
    public List<SemanticError> Errors { get; } = new List<SemanticError>();

    /// <summary> The semantic warnings in the compilation. </summary>
    public List<SemanticWarning> Warnings { get; } = new List<SemanticWarning>();

    private TweetyLangCompilation(IEnumerable<TweetyLangSyntaxTree> trees)
    {
        syntaxTrees = new List<TweetyLangSyntaxTree>(trees);

        foreach (var tree in syntaxTrees)
            symbolDictionaries[tree] = new SymbolDictionary(this, tree);
    }

    public static TweetyLangCompilation Create(string assemblyName, IEnumerable<TweetyLangSyntaxTree> syntaxTrees)
    {
        TweetyLangCompilation compilation = new TweetyLangCompilation(syntaxTrees);

        // Run semantic checks
        SemanticAnalyzer analyzer = new SemanticAnalyzer(compilation);

        foreach (var tree in syntaxTrees)
        {
            analyzer.Analyze(tree);

            // Add the analyzer errors and warnings to the compilation
            compilation.Errors.AddRange(analyzer.Errors);
            compilation.Warnings.AddRange(analyzer.Warnings);
        }

        return compilation;
    }


    /// <summary>
    /// Returns the symbol dictionary for a specific syntax tree.
    /// </summary>
    public SymbolDictionary GetSymbolDictionary(TweetyLangSyntaxTree tree)
    {
        if (!symbolDictionaries.TryGetValue(tree, out var dict))
            throw new InvalidOperationException("Tree is not part of this compilation.");

        return dict;
    }

    /// <summary>
    /// Returns all symbols of a given type across the entire compilation.
    /// </summary>
    public IEnumerable<T> GetAllSymbols<T>() where T : ISymbol
    {
        return symbolDictionaries.Values
            .SelectMany(dict => dict.GetAllSymbols<T>());
    }
}
