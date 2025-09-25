
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using TweetyLang.AST;
using TweetyLang.Parser.ErrorListeners;
using TweetyLang.Parser.Semantics;

namespace TweetyLang.Parser.AST;

public class TweetyLangSyntaxTree
{
    /// <summary>
    /// The root of the parse tree.
    /// </summary>
    public ProgramNode Root { get; set; } = null!;

    /// <summary>
    /// The errors found during parsing.
    /// </summary>
    public IEnumerable<SemanticError> Errors = null!;

    /// <summary>
    /// The warnings found during parsing.
    /// </summary>
    public IEnumerable<SemanticWarning> Warnings = null!;

    private TweetyLangSyntaxTree() { }

    /// <summary>
    /// Parses TweetyLang source code and returns a syntax tree.
    /// </summary>
    /// <param name="text">The TweetyLang source code to parse.</param>
    /// <returns>The syntax tree.</returns>
    public static TweetyLangSyntaxTree ParseText(string text)
    {
        var tree = new TweetyLangSyntaxTree();

        // ANTLR setup
        var input = new AntlrInputStream(text);
        var lexer = new TweetyLangLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new TweetyLangParser(tokens);

        // Remove default error listeners
        lexer.RemoveErrorListeners();
        parser.RemoveErrorListeners();

        // Attach custom error listeners
        var parserListener = new ParserErrorListener();
        parser.AddErrorListener(parserListener);

        var lexerListener = new LexerErrorListener();
        lexer.AddErrorListener(lexerListener);

        // Parse
        var programContext = parser.program();

        // Build AST
        tree.Root = new AstBuilder().Visit(programContext) as ProgramNode ?? throw new InvalidOperationException("Failed to build AST");

        tree.Errors = Enumerable.Empty<SemanticError>();
        tree.Warnings = Enumerable.Empty<SemanticWarning>();

        // Merge lexer and parser errors
        tree.Errors = tree.Errors
            .Concat(lexerListener.Errors)
            .Concat(parserListener.Errors);

        // Run analyzers
        var analyzer = new SemanticAnalyzer();
        analyzer.Analyze(tree.Root);

        // Merge semantic errors and warnings
        tree.Errors = tree.Errors.Concat(analyzer.Errors);
        tree.Warnings = tree.Warnings.Concat(analyzer.Warnings);

        return tree;
    }
}
