namespace TweetyLang.AST;

public class TypeReference
{
    public string BaseType { get; set; }
    public int PointerLevel { get; set; }

    public TypeReference(string baseType, int pointerLevel = 0)
    {
        BaseType = baseType;
        PointerLevel = pointerLevel;
    }

    public override string ToString() => BaseType + new string('*', PointerLevel);

    public static TypeReference Void => new("void");
    public static TypeReference I32 => new("i32");
}

[Flags]
public enum Modifiers
{
    None,
    Export
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
        if (child != null)
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
            if (c != null) c.Parent = this;
            yield return c;
        }
    }
}

public class ProgramNode : AstNode
{
    public List<ImportNode> Imports { get; set; } = new();
    public List<ModuleNode> Modules { get; set; } = new();
}

public class ImportNode : AstNode
{
    public string ModuleName { get; set; }
}

public class ModuleNode : AstNode
{
    /// <summary> The name of the module. </summary>
    public string Name { get; set; }

    public List<FunctionNode> Functions { get; set; } = new();
}

public class FunctionNode : AstNode
{
    /// <summary> The name of the function. </summary>
    public string Name { get; set; }

    /// <summary> The fully qualified name of the function. </summary>
    public string FullName => Parent is ModuleNode module ? $"{module.Name}::{Name}" : Name;

    public TypeReference ReturnType { get; set; }
    public Modifiers Modifiers { get; set; }
    public List<ParameterNode> Parameters { get; set; } = new();
    public List<StatementNode> Body { get; set; } = new();
}

public class ParameterNode : AstNode
{
    public string Name { get; set; }
    public TypeReference Type { get; set; }
}

public abstract class StatementNode : AstNode { }

public class DeclarationNode : StatementNode
{
    /// <summary> The name of the variable being declared. </summary>
    public string Name { get; set; }

    public TypeReference Type { get; set; }
    public ExpressionNode Expression { get; set; }
}

public class AssignmentNode : StatementNode
{
    /// <summary> The name of the variable being assigned. </summary>
    public string Name { get; set; }

    public ExpressionNode Expression { get; set; }
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

public class BinaryExpressionNode : ExpressionNode
{
    public string Operator { get; set; }
    public ExpressionNode Left { get; set; }
    public ExpressionNode Right { get; set; }
}

public class FunctionCallNode : ExpressionNode
{
    public string Name { get; set; }
    public List<ExpressionNode> Arguments { get; set; } = new();
}

public class IfNode : StatementNode
{
    public ExpressionNode Condition { get; set; }
    public List<StatementNode> ThenBlock { get; set; } = new();
    public List<StatementNode>? ElseBlock { get; set; }
}