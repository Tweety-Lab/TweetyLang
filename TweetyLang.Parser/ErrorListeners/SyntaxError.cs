namespace TweetyLang.Parser.ErrorListeners;

/// <summary>
/// An error that occured during AST parsing.
/// </summary>
public class SyntaxError
{
    /// <summary>The line of source code that the error occured on.</summary>
    public int Line { get; set; }

    /// <summary>The column of source code that the error occured on.</summary>
    public int Column { get; set; }

    /// <summary>The message of the error.</summary>
    public string Message { get; set; }

    public SyntaxError(int line, int column, string message) => (Line, Column, Message) = (line, column, message);
}
