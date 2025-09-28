
namespace TweetyLang.Compiler.Symbols;

/// <summary>
/// A Function Declaration.
/// </summary>
public interface IFunctionSymbol : ISymbol
{
    public new string Name { get; }
}

internal class FunctionSymbol : IFunctionSymbol
{
    /// <inheritdoc/>
    public string Name { get; }

    public FunctionSymbol(string name) => Name = name;
}