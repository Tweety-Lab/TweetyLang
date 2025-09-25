
using Antlr4.Runtime;
using TweetyLang.Parser.Semantics;

namespace TweetyLang.Parser.ErrorListeners;

/// <summary>
/// Error listener for ANTLR Parser Syntax Errors.
/// </summary>
internal class ParserErrorListener : BaseErrorListener
{
    /// <summary>
    /// The list of al caught syntax errors.
    /// </summary>
    public List<SemanticError> Errors { get; } = new();

    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        Errors.Add(new SemanticError(line, charPositionInLine, msg));
    }
}
