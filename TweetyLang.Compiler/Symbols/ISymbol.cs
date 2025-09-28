
namespace TweetyLang.Compiler.Symbols;

/// <summary>
/// A Base Symbol.
/// </summary>
public interface ISymbol
{
    /// <summary> The name of the symbol. </summary>
    string Name { get; }
}
