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

public abstract class AstNode
{
    public int SourceLine { get; set; }
    public int SourceColumn { get; set; }
}

public class ProgramNode : AstNode
{
    public List<ModuleNode> Modules { get; set; } = new();
}

public class ModuleNode : AstNode
{
    public string Name { get; set; }
    public List<FunctionNode> Functions { get; set; } = new();
}

public class FunctionNode : AstNode
{
    public string Name { get; set; }
    public TypeReference ReturnType { get; set; }
    public string AccessModifier { get; set; }
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
    public string Name { get; set; }
    public TypeReference Type { get; set; }
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