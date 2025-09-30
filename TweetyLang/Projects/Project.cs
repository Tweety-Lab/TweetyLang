
namespace TweetyLang.Projects;

/// <summary>
/// Serializable representation of a TweetyLang project file.
/// </summary>
internal class Project
{
    /// <summary>The project's output type (i.e. exe, lib, etc).</summary>
    public string? OutputType { get; set; }

    /// <summary>The project's dependencies.</summary>
    public Dictionary<string, ProjectDependency>? Dependencies { get; set; }
}

internal struct ProjectDependency
{
    public string Path { get; set; }
    public string LinkType { get; set; }
}
