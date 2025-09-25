using Antlr4.Runtime;
using TweetyLang.Parser.Semantics;

namespace TweetyLang.Parser.ErrorListeners;

/// <summary>
/// Error listener for ANTLR Lexer Syntax Errors.
/// </summary>
internal class LexerErrorListener : IAntlrErrorListener<int>
{
    /// <summary>
    /// The list of al caught syntax errors.
    /// </summary>
    public List<SemanticError> Errors { get; } = new();

    public void SyntaxError(TextWriter output,
                            IRecognizer recognizer,
                            int offendingSymbol,
                            int line,
                            int charPositionInLine,
                            string msg,
                            RecognitionException e)
    {
        Errors.Add(new SemanticError(line, charPositionInLine, msg));
    }
}
