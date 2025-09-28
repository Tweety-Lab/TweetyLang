
using TweetyLang.AST;

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
}

public interface IModuleSymbol : ISymbol, IEquatable<IModuleSymbol> { }
public interface IFunctionSymbol : ISymbol, IEquatable<IFunctionSymbol> { }
public interface IParameterSymbol : ISymbol, IEquatable<IParameterSymbol> { }

public interface IVariableSymbol : ISymbol, IEquatable<IVariableSymbol>
{
    /// <summary> The type of the variable. </summary>
    public TypeReference Type { get; set; }
}

internal class ModuleSymbol : IModuleSymbol
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsExport { get; set; }

    /// <inheritdoc/>
    public bool IsExtern { get; set; }

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


internal class FunctionSymbol : IFunctionSymbol
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsExport { get; set; }

    /// <inheritdoc/>
    public bool IsExtern { get; set; }

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


internal class ParameterSymbol : IParameterSymbol
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsExport { get; set; }

    /// <inheritdoc/>
    public bool IsExtern { get; set; }

    public ParameterSymbol(string name) => Name = name;

    // IEquatable
    public override bool Equals(object? obj) => Equals(obj as IParameterSymbol);

    public bool Equals(IParameterSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name && IsExport == other.IsExport && IsExtern == other.IsExtern;
    }

    public override int GetHashCode() => HashCode.Combine(Name, IsExport, IsExtern);
}


internal class VariableSymbol : IVariableSymbol
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsExport { get; set; }

    /// <inheritdoc/>
    public bool IsExtern { get; set; }

    /// <inheritdoc/>
    public TypeReference Type { get; set; }

    public VariableSymbol(string name) => Name = name;

    // IEquatable
    public override bool Equals(object? obj) => Equals(obj as IVariableSymbol);

    public bool Equals(IVariableSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name && IsExport == other.IsExport && IsExtern == other.IsExtern;
    }

    public override int GetHashCode() => HashCode.Combine(Name, IsExport, IsExtern);
}