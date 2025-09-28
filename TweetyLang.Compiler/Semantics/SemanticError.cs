
namespace TweetyLang.Parser.Semantics;


/// <summary>
/// An error that occured during semantic analysis.
/// </summary>
public class SemanticError
{
    /// <summary>The line of source code that the error occured on.</summary>
    public int Line { get; set; }

    /// <summary>The column of source code that the error occured on.</summary>
    public int Column { get; set; }

    /// <summary>The message of the error.</summary>
    public string Message { get; set; }

    public SemanticError(int line, int column, string message) => (Line, Column, Message) = (line, column, message);
}
