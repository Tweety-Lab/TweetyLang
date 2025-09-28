
namespace TweetyLang.Compiler.Symbols;

/// <summary>
/// A Function Declaration.
/// </summary>
public interface IFunctionSymbol : ISymbol
{
    public new string Name { get; }

    /// <summary> Whether the function is marked with 'export'. </summary>
    public bool IsExport { get; set; }

    /// <summary> Whether the function is marked with 'extern'. </summary>
    public bool IsExtern { get; set; }
}

internal class FunctionSymbol : IFunctionSymbol
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsExport { get; set; }

    /// <inheritdoc/>
    public bool IsExtern { get; set; }

    public FunctionSymbol(string name) => Name = name;
}