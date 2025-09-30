using System.Xml.Linq;
using TweetyLang.Parser.AST;

namespace TweetyLang.AST;

public class TypeReference : IEquatable<TypeReference>
{
    /// <summary> The base type of the type reference. </summary>
    public string BaseType { get; set; }

    /// <summary> The number of pointer levels in the type reference. </summary>
    public int PointerLevel { get; set; }

    /// <summary> Creates a new instance of <see cref="TypeReference"/> </summary>
    public TypeReference(string baseType, int pointerLevel = 0)
    {
        BaseType = baseType;
        PointerLevel = pointerLevel;
    }

    /// <summary> Returns a string representation of the type reference. </summary>
    public override string ToString() => BaseType + new string('*', PointerLevel);

    /// <summary> Compares two type references for equality. </summary>
    public bool Equals(TypeReference? obj)
    {
        if (obj is not TypeReference other) return false;
        return BaseType == other.BaseType && PointerLevel == other.PointerLevel;
    }

    /// <summary> Compares two type references for equality. </summary>
    public override bool Equals(object obj)
    {
        return Equals(obj as TypeReference);
    }

    public override int GetHashCode() => HashCode.Combine(BaseType, PointerLevel);

    public static bool operator ==(TypeReference? left, TypeReference? right) =>
        Equals(left, right);

    public static bool operator !=(TypeReference? left, TypeReference? right) =>
        !Equals(left, right);

    public static TypeReference Void => new("void");
    public static TypeReference I32 => new("i32");
    public static TypeReference Bool => new("bool");
    public static TypeReference Char => new("char");
}

[Flags]
public enum Modifiers
{
    None,
    Export,
    Extern
}

public abstract class AstNode
{
    public int SourceLine { get; set; }
    public int SourceColumn { get; set; }

    /// <summary> The parent node of this node. </summary>
    public AstNode? Parent { get; internal set; }

    /// <summary>
    /// Returns all ancestors (parents) of this node.
    /// </summary>
    /// <returns>IEnumerable of parent AstNodes.</returns>
    public IEnumerable<AstNode> Ancestors()
    {
        var current = Parent;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <typeparam name="T">Type of child node.</typeparam>
    /// <param name="child">Child node.</param>
    /// <returns>Child node.</returns>
    public T AddChild<T>(T child) where T : AstNode
    {
        if (child == null) return null!;

        child.Parent = this;

        return child;
    }


    /// <summary>
    /// Adds multiple child nodes to this node.
    /// </summary>
    /// <typeparam name="T">Type of child node.</typeparam>
    /// <param name="children">Child nodes.</param>
    /// <returns>Child nodes.</returns>
    public IEnumerable<T> AddChildren<T>(IEnumerable<T> children) where T : AstNode
    {
        foreach (var c in children)
        {
            if (c != null)
            {
                c.Parent = this;
            }

            yield return c;
        }
    }

    /// <summary>
    /// Returns the direct children of this node.
    /// </summary>
    public virtual IEnumerable<AstNode> GetChildren() => Enumerable.Empty<AstNode>();

    /// <summary>
    /// Returns all descendants of this node.
    /// </summary>
    public IEnumerable<AstNode> Descendants()
    {
        foreach (var child in GetChildren())
        {
            yield return child;
            foreach (var desc in child.Descendants())
                yield return desc;
        }
    }

    /// <summary>
    /// Returns this node and all its descendants.
    /// </summary>
    public IEnumerable<AstNode> DescendantsAndSelf()
    {
        yield return this;
        foreach (var desc in Descendants())
            yield return desc;
    }
}

public class ProgramNode : AstNode
{
    public List<ImportNode> Imports { get; set; } = new();
    public List<ModuleNode> Modules { get; set; } = new();

    public override IEnumerable<AstNode> GetChildren()
    {
        foreach (var import in Imports) yield return import;
        foreach (var module in Modules) yield return module;
    }
}

public class ImportNode : AstNode
{
    /// <summary> The name of the module being imported. </summary>
    public string ModuleName { get; set; }

    public override IEnumerable<AstNode> GetChildren() => Enumerable.Empty<AstNode>();
}

public class ModuleNode : AstNode
{
    /// <summary> The name of the module. </summary>
    public string Name { get; set; }

    public List<FunctionNode> Functions { get; set; } = new();
    public List<StructNode> Structs { get; set; } = new();

    public override IEnumerable<AstNode> GetChildren() => Functions;
}

public class StructNode : AstNode
{
    /// <summary> The name of the struct. </summary>
    public string Name { get; set; }

    public Modifiers Modifiers { get; set; }

    public List<FieldDeclarationNode> Fields { get; set; } = new();
    public List<FunctionNode> Functions { get; set; } = new();


}

public class FieldDeclarationNode : AstNode
{
    /// <summary> The name of the field. </summary>
    public string Name { get; set; }

    public TypeReference Type { get; set; }
    public ExpressionNode? Expression { get; set; }

    public override IEnumerable<AstNode> GetChildren() => new[] { Expression };
}

public class FunctionNode : AstNode
{
    /// <summary> The name of the function. </summary>
    public string Name { get; set; }

    public TypeReference ReturnType { get; set; }
    public Modifiers Modifiers { get; set; }
    public List<ParameterNode> Parameters { get; set; } = new();
    public List<StatementNode>? Body { get; set; }

    public override IEnumerable<AstNode> GetChildren()
    {
        foreach (var param in Parameters) yield return param;
        if (Body != null)
        {
            foreach (var stmt in Body) yield return stmt;
        }
    }
}

public class ParameterNode : AstNode
{
    public string Name { get; set; }
    public TypeReference Type { get; set; }
}

public abstract class StatementNode : AstNode { }

public class LocalDeclarationNode : StatementNode
{
    /// <summary> The name of the variable being declared. </summary>
    public string Name { get; set; }

    public TypeReference Type { get; set; }
    public ExpressionNode Expression { get; set; }

    public override IEnumerable<AstNode> GetChildren() => new[] { Expression };
}

public class AssignmentNode : StatementNode
{
    /// <summary> The name of the variable being assigned. </summary>
    public string Name { get; set; }

    public ExpressionNode Expression { get; set; }

    public override IEnumerable<AstNode> GetChildren() => new[] { Expression };
}

public class ReturnNode : StatementNode
{
    public ExpressionNode Expression { get; set; }
}

public abstract class ExpressionNode : AstNode { }

public class IdentifierNode : ExpressionNode
{
    public string Name { get; set; }

}

public class BooleanLiteralNode : ExpressionNode
{
    public bool Value { get; set; }
}

public class IntegerLiteralNode : ExpressionNode
{
    public int Value { get; set; }
}

public class CharacterLiteralNode : ExpressionNode
{
    public char Value { get; set; }
}

public class StringLiteralNode : ExpressionNode
{
    public string Value { get; set; }
}

public class BinaryExpressionNode : ExpressionNode
{
    public string Operator { get; set; }
    public ExpressionNode Left { get; set; }
    public ExpressionNode Right { get; set; }

    public override IEnumerable<AstNode> GetChildren()
    {
        yield return Left;
        yield return Right;
    }
}

public class FunctionCallNode : ExpressionNode
{
    public string Name { get; set; }
    public List<ExpressionNode> Arguments { get; set; } = new();

    public override IEnumerable<AstNode> GetChildren() => Arguments;
}

public class IfNode : StatementNode
{
    public ExpressionNode Condition { get; set; }
    public List<StatementNode> ThenBlock { get; set; } = new();
    public List<StatementNode>? ElseBlock { get; set; }

    public override IEnumerable<AstNode> GetChildren()
    {
        yield return Condition;
        foreach (var stmt in ThenBlock) yield return stmt;
        if (ElseBlock != null)
        {
            foreach (var stmt in ElseBlock) yield return stmt;
        }
    }
}

public class ExpressionStatementNode : StatementNode
{
    public ExpressionNode Expression { get; set; }

    public override IEnumerable<AstNode> GetChildren() => new[] { Expression };
}