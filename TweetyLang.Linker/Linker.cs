using LLVMSharp.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TweetyLang.Linker;

public enum AssemblyType
{
    /// <summary> A runnable console application. </summary>
    Application,

    /// <summary> A reusable dynamically linked library. </summary>
    DynamicLibrary,

    /// <summary> A reusable statically linked library. </summary>
    StaticLibrary
}

public static class Linker
{
    /// <summary>
    /// Converts an LLVM module to an object file.
    /// </summary>
    /// <param name="module">LLVM Module.</param>
    /// <param name="objectOutputPath">Path to write the object file to.</param>
    /// <param name="targetTriple">Target triple (i.e., "x86_64-pc-windows-msvc").</param>
    public static void ModuleToObjectFile(LLVMModuleRef module, string objectOutputPath, string targetTriple)
    {
        // TODO: We should parse targetTriple and initialize appropriate targets
        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmPrinter();
        LLVM.InitializeNativeAsmParser();

        module.Target = targetTriple;
        LLVMTargetRef target = LLVMTargetRef.GetTargetFromTriple(targetTriple);

        LLVMTargetMachineRef targetMachine = target.CreateTargetMachine(targetTriple, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);

        targetMachine.EmitToFile(module, objectOutputPath, LLVMCodeGenFileType.LLVMObjectFile);
    }

    /// <summary>
    /// Converts object files to an executable or library using the system's native linker.
    /// </summary>
    /// <param name="objectFiles">Object files to link.</param>
    /// <param name="outputPath">Path to write the final output.</param>
    /// <param name="type">Type of assembly to generate.</param>
    public static void ObjectFilesToAssembly(string[] objectFiles, string outputPath, AssemblyType type)
    {
        if (objectFiles == null || objectFiles.Length == 0)
            throw new ArgumentException("At least one object file is required.", nameof(objectFiles));

        foreach (var obj in objectFiles)
            if (!File.Exists(obj))
                throw new FileNotFoundException("Object file not found.", obj);

        // Determine correct file extension
        string extension = type switch
        {
            AssemblyType.Application => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "",
            AssemblyType.DynamicLibrary => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" :
                                          RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so",
            AssemblyType.StaticLibrary => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".lib" : ".a",
            _ => throw new NotSupportedException($"Unsupported assembly type: {type}")
        };

        outputPath = Path.ChangeExtension(outputPath, extension);

        string linker;
        string args;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string path = GetLinkerPath();
            switch (type)
            {
                case AssemblyType.Application:
                    linker = path;
                    args = $"/OUT:\"{outputPath}\" /ENTRY:main {string.Join(" ", objectFiles)} /SUBSYSTEM:CONSOLE"; // IMPORTANT: Clarify entry point as main
                    break;

                case AssemblyType.DynamicLibrary:
                    linker = path;
                    args = $"/DLL /OUT:\"{outputPath}\" /NOENTRY {string.Join(" ", objectFiles)}";
                    break;

                case AssemblyType.StaticLibrary:
                    linker = path;
                    args = $"/OUT:\"{outputPath}\" /NOENTRY {string.Join(" ", objectFiles)}";
                    break;

                default:
                    throw new NotSupportedException($"Unsupported assembly type: {type}");
            }
        }
        else // Linux/macOS
        {
            linker = "clang";
            switch (type)
            {
                case AssemblyType.Application:
                    args = $"-o \"{outputPath}\" {string.Join(" ", objectFiles)}";
                    break;

                case AssemblyType.DynamicLibrary:
                    args = $"-shared -o \"{outputPath}\" {string.Join(" ", objectFiles)}";
                    break;

                case AssemblyType.StaticLibrary:
                    linker = "ar";
                    args = $"rcs \"{outputPath}\" {string.Join(" ", objectFiles)}";
                    break;

                default:
                    throw new NotSupportedException($"Unsupported assembly type: {type}");
            }
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = linker,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
                         ?? throw new InvalidOperationException($"Failed to start {linker} process.");

        process.WaitForExit();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        if (process.ExitCode != 0)
        {
            Console.WriteLine(output);
            Console.Error.WriteLine(error);
            throw new InvalidOperationException($"{linker} failed with exit code {process.ExitCode}.");
        }

        Console.WriteLine(output);
    }

    // Use Vswhere to get the path to the windows linker
    private static string GetLinkerPath()
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
