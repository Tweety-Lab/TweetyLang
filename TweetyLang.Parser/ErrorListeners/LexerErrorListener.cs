using Antlr4.Runtime;

namespace TweetyLang.Parser.ErrorListeners;

/// <summary>
/// Error listener for ANTLR Lexer Syntax Errors.
/// </summary>
internal class LexerErrorListener : IAntlrErrorListener<int>
{
    /// <summary>
    /// The list of all caught syntax errors.
    /// </summary>
    public List<SyntaxError> Errors { get; } = new();

    public void SyntaxError(TextWriter output,
                            IRecognizer recognizer,
                            int offendingSymbol,
                            int line,
                            int charPositionInLine,
                            string msg,
                            RecognitionException e)
    {
        Errors.Add(new SyntaxError(line, charPositionInLine, msg));
    }
}
