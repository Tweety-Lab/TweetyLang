using Antlr4.Runtime;

namespace TweetyLang.Parser.Errors;

/// <summary>
/// Error listener for ANTLR Lexer Syntax Errors.
/// </summary>
internal class LexerErrorListener : IAntlrErrorListener<int>
{
    public List<string> Errors { get; } = new List<string>();

    public void SyntaxError(TextWriter output,
                            IRecognizer recognizer,
                            int offendingSymbol,
                            int line,
                            int charPositionInLine,
                            string msg,
                            RecognitionException e)
    {
        Errors.Add($"Lexer error at {line}:{charPositionInLine} {msg}");
    }
}
