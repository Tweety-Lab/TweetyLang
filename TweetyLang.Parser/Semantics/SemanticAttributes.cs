

namespace TweetyLang.Parser.Semantics;

/// <summary>
/// Marks a class as a semantic analyzer.
/// </summary>
/// <remarks> Classes marked with this will automatically be added to the semantic analyzer.</remarks>
[AttributeUsage(AttributeTargets.Class)]
internal class SemanticAnalyzerAttribute : Attribute { }
