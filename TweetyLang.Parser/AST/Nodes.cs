namespace TweetyLang.AST;

public abstract class AstNode { }

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
    public string ReturnType { get; set; }
    public string AccessModifier { get; set; }
    public List<ParameterNode> Parameters { get; set; } = new();
    public List<StatementNode> Body { get; set; } = new();
}

public class ParameterNode : AstNode
{
    public string Name { get; set; }
    public string Type { get; set; }
}

public abstract class StatementNode : AstNode { }

public class DeclarationNode : StatementNode
{
    public string Name { get; set; }
    public string Type { get; set; }
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