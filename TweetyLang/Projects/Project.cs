
namespace TweetyLang.Projects;

/// <summary>
/// Serializable representation of a TweetyLang project file.
/// </summary>
internal class Project
{
    /// <summary>The project's output type (i.e. exe, lib, etc).</summary>
    public string? OutputType { get; set; }
}
