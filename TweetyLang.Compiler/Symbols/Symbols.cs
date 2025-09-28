
namespace TweetyLang.Compiler.Symbols;

/// <summary>
/// A Base Symbol.
/// </summary>
public interface ISymbol
{
    /// <summary> The name of the symbol. </summary>
    string Name { get; }

    /// <summary> Whether the symbol is marked with 'export'. </summary>
    public bool IsExport { get; set; }

    /// <summary> Whether the symbol is marked with 'extern'. </summary>
    public bool IsExtern { get; set; }

    /// <summary> The module that contains the symbol. </summary>
    IModuleSymbol? ContainingModule { get; }
}

public interface IModuleSymbol : ISymbol, IEquatable<IModuleSymbol>
{
    public new string Name { get; }
}

internal class ModuleSymbol : IModuleSymbol
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsExport { get; set; }

    /// <inheritdoc/>
    public bool IsExtern { get; set; }

    public IModuleSymbol? ContainingModule => throw new NotImplementedException();

    public ModuleSymbol(string name) => Name = name;

    // IEquatable
    public override bool Equals(object? obj) => Equals(obj as IModuleSymbol);

    public bool Equals(IModuleSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name && IsExport == other.IsExport && IsExtern == other.IsExtern;
    }

    public override int GetHashCode() => HashCode.Combine(Name, IsExport, IsExtern);
}


public interface IFunctionSymbol : ISymbol, IEquatable<IFunctionSymbol>
{
}

internal class FunctionSymbol : IFunctionSymbol
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsExport { get; set; }

    /// <inheritdoc/>
    public bool IsExtern { get; set; }

    public IModuleSymbol? ContainingModule => throw new NotImplementedException();

    public FunctionSymbol(string name) => Name = name;

    // IEquatable
    public override bool Equals(object? obj) => Equals(obj as IFunctionSymbol);

    public bool Equals(IFunctionSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name && IsExport == other.IsExport && IsExtern == other.IsExtern;
    }

    public override int GetHashCode() => HashCode.Combine(Name, IsExport, IsExtern);
}