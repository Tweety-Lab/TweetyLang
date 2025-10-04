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
    bool IsExport { get; set; }

    /// <summary> Whether the symbol is marked with 'extern'. </summary>
    bool IsExtern { get; set; }
}

internal class TypeSymbol : ISymbol, IEquatable<TypeSymbol>
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsExport { get; set; }

    /// <inheritdoc/>
    public bool IsExtern { get; set; }

    public TypeSymbol(string name) => Name = name;

    public override bool Equals(object? obj) => Equals(obj as TypeSymbol);

    public bool Equals(TypeSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && IsExport == other.IsExport && IsExtern == other.IsExtern;
    }

    public override int GetHashCode() => HashCode.Combine(Name, IsExport, IsExtern);
}

internal class ModuleSymbol : ISymbol, IEquatable<ModuleSymbol>
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsExport { get; set; }

    /// <inheritdoc/>
    public bool IsExtern { get; set; }

    public ModuleSymbol(string name) => Name = name;

    public override bool Equals(object? obj) => Equals(obj as ModuleSymbol);

    public bool Equals(ModuleSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && IsExport == other.IsExport && IsExtern == other.IsExtern;
    }

    public override int GetHashCode() => HashCode.Combine(Name, IsExport, IsExtern);
}

internal class FunctionSymbol : ISymbol, IEquatable<FunctionSymbol>
{
    public string Name { get; }
    public bool IsExport { get; set; }
    public bool IsExtern { get; set; }

    public FunctionSymbol(string name) => Name = name;

    public override bool Equals(object? obj) => Equals(obj as FunctionSymbol);

    public bool Equals(FunctionSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && IsExport == other.IsExport && IsExtern == other.IsExtern;
    }

    public override int GetHashCode() => HashCode.Combine(Name, IsExport, IsExtern);
}

internal class ParameterSymbol : ISymbol, IEquatable<ParameterSymbol>
{
    public string Name { get; }
    public bool IsExport { get; set; }
    public bool IsExtern { get; set; }

    public ParameterSymbol(string name) => Name = name;

    public override bool Equals(object? obj) => Equals(obj as ParameterSymbol);

    public bool Equals(ParameterSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && IsExport == other.IsExport && IsExtern == other.IsExtern;
    }

    public override int GetHashCode() => HashCode.Combine(Name, IsExport, IsExtern);
}

internal class VariableSymbol : ISymbol, IEquatable<VariableSymbol>
{
    public string Name { get; }
    public bool IsExport { get; set; }
    public bool IsExtern { get; set; }

    /// <summary> The type of the variable. </summary>
    public TypeReference Type { get; set; }

    public VariableSymbol(string name, TypeReference type)
    {
        Name = name;
        Type = type;
    }

    public override bool Equals(object? obj) => Equals(obj as VariableSymbol);

    public bool Equals(VariableSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && IsExport == other.IsExport && IsExtern == other.IsExtern;
    }

    public override int GetHashCode() => HashCode.Combine(Name, IsExport, IsExtern);
}
