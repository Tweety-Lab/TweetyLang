
namespace TweetyLang.Parser.Semantics;

/// <summary>
/// A warning that occured during semantic analysis.
/// </summary>
public class SemanticWarning
{
    /// <summary>The line of source code that the warning occured on.</summary>
    public int Line { get; }

    /// <summary>The column of source code that the warning occured on.</summary>
    public int Column { get; }

    /// <summary>The message of the warning.</summary>
    public string Message { get; } = string.Empty;

    public SemanticWarning(int line, int column, string message)
    {
        Line = line;
        Column = column;
        Message = message;
    }
}
