
namespace TweetyLang.Parser.Semantics;


/// <summary>
/// An exception that occured during semantic analysis.
/// </summary>
public class SemanticException : Exception
{
    /// <summary>The line of source code that the exception occured on.</summary>
    public int Line { get; set; }

    /// <summary>The column of source code that the exception occured on.</summary>
    public int Column { get; set; }

    public SemanticException(int line, int column, string message) : base(message) => (Line, Column) = (line, column);
}
