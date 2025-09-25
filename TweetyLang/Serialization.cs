using Tomlyn;
using TweetyLang.Projects;

namespace TweetyLang;

/// <summary>
/// Utility for serializing and deserializing TweetyLang TOML files.
/// </summary>
internal static class Serialization
{
    /// <summary>
    /// Loads a project from a TOML file.
    /// </summary>
    /// <param name="path">Path to the TOML file.</param>
    /// <returns>Project.</returns>
    public static Project LoadProject(string path)
    {
        TomlModelOptions options = new TomlModelOptions { ConvertPropertyName = name => name };
        return Toml.ToModel<Project>(File.ReadAllText(path), options: options);
    }

    /// <summary>
    /// Serializes a project to a TOML file.
    /// </summary>
    /// <param name="project">Project.</param>
    /// <param name="path">Path to the TOML file.</param>
    public static void SaveProject(Project project, string path)
    {
        TomlModelOptions options = new TomlModelOptions { ConvertPropertyName = name => name };
        File.WriteAllText(path, Toml.FromModel(project, options: options));
    }
}
