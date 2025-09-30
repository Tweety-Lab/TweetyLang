
using TweetyLang.AST;
using TweetyLang.Parser.AST;

namespace TweetyLang.Compiler.Symbols;

/// <summary>
/// Housing class for symbols in the AST.
/// </summary>
public class SymbolDictionary
{
    private readonly TweetyLangCompilation compilation;
    private readonly TweetyLangSyntaxTree tree;
    private readonly Dictionary<AstNode, ISymbol> map = new();

    internal SymbolDictionary(TweetyLangCompilation compilation, TweetyLangSyntaxTree tree)
    {
        this.compilation = compilation;
        this.tree = tree;
    }

    /// <summary>
    /// Returns the symbol for a node.
    /// </summary>
    /// <param name="node">Node to get symbol for.</param>
    /// <returns>Symbol.</returns>
    public ISymbol? GetDeclaredSymbol(AstNode node)
    {
        if (map.TryGetValue(node, out var symbol))
            return symbol;

        var computed = ComputeDeclaredSymbol(node);
        if (computed != null)
            map[node] = computed;

        return computed;
    }

    /// <summary>
    /// Returns the symbol for a node.
    /// </summary>
    /// <typeparam name="T">Type of symbol.</typeparam>
    /// <param name="node">Node to get symbol for.</param>
    /// <returns>Symbol.</returns>
    public T? GetDeclaredSymbol<T>(AstNode node) where T : ISymbol => (T?)GetDeclaredSymbol(node);

    public IEnumerable<T> GetAllSymbols<T>() where T : ISymbol
    {
        foreach (var node in tree.Root.DescendantsAndSelf())
        {
            var symbol = GetDeclaredSymbol(node);
            if (symbol is T typedSymbol)
                yield return typedSymbol;
        }
    }

    private ISymbol? ComputeDeclaredSymbol(AstNode node) => node switch
    {
        ModuleNode m => new ModuleSymbol(m.Name),
        FunctionNode f => new FunctionSymbol(f.Name)
        {
            IsExport = f.Modifiers.HasFlag(Modifiers.Export),
            IsExtern = f.Modifiers.HasFlag(Modifiers.Extern)
        },
        ParameterNode p => new ParameterSymbol(p.Name),
        LocalDeclarationNode v => new VariableSymbol(v.Name) { Type = v.Type },
        _ => null
    };
}
