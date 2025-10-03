
using System.Diagnostics;

namespace TweetyLang.Linker;

internal enum TargetOS
{
    Windows,
    OSX,
    MacOS
}

internal static class Utility
{
    /// <summary>
    /// Gets the OS from the target triple.
    /// </summary>
    /// <param name="targetTriple">Target triple (i.e., "x86_64-pc-windows-msvc").</param>
    /// <returns>OS.</returns>
    /// <exception cref="NotSupportedException">Unsupported target triple.</exception>
    public static TargetOS GetOSFromTriple(string targetTriple)
    {
        return targetTriple switch
        {
            string s when s.Contains("windows") => TargetOS.Windows,
            string s when s.Contains("linux") => TargetOS.OSX,
            string s when s.Contains("darwin") => TargetOS.MacOS,
            string s when s.Contains("macos") => TargetOS.MacOS,
            _ => throw new NotSupportedException($"Unsupported target triple: {targetTriple}")
        };
    }

    /// <summary>
    /// Gets the path to the Windows linker using VS where.
    /// </summary>
    /// <returns>Path to the linker.</returns>
    public static string GetWindowsLinkerPath()
    {
        string vsWhere = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";

        var psi = new ProcessStartInfo
        {
            FileName = vsWhere,
            Arguments = "-latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        string vsPath = process.StandardOutput.ReadLine();
        process.WaitForExit();

        string msvcVersionDir = Directory.GetDirectories(Path.Combine(vsPath, "VC", "Tools", "MSVC"))[0];
        string linkPath = Path.Combine(msvcVersionDir, "bin", "Hostx64", "x64", "link.exe");

        return linkPath;
    }
}
