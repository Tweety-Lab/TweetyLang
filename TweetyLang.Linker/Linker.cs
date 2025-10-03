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
        LLVM.InitializeAllTargetInfos();
        LLVM.InitializeAllTargetMCs();
        LLVM.InitializeAllAsmParsers();
        LLVM.InitializeAllAsmPrinters();

        LLVM.InitializeAllTargets();

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
    public static void ObjectFilesToAssembly(string[] objectFiles, string outputPath, AssemblyType type, string targetTriple)
    {
        if (objectFiles == null || objectFiles.Length == 0)
            throw new ArgumentException("At least one object file is required.", nameof(objectFiles));

        foreach (var obj in objectFiles)
            if (!File.Exists(obj))
                throw new FileNotFoundException("Object file not found.", obj);

        TargetOS targetOS = Utility.GetOSFromTriple(targetTriple);

        string extension = type switch
        {
            _ when targetOS == TargetOS.Web => ".wasm",

            AssemblyType.Application => targetOS == TargetOS.Windows ? ".exe" : "",
            AssemblyType.DynamicLibrary => targetOS == TargetOS.Windows ? ".dll" :
                                          targetOS == TargetOS.OSX ? ".dylib" : ".so",
            AssemblyType.StaticLibrary => targetOS == TargetOS.Windows ? ".lib" : ".a",

            _ => throw new NotSupportedException($"Unsupported assembly type: {type}")
        };

        outputPath = Path.ChangeExtension(outputPath, extension);

        string linker;
        string args;

        if (targetOS == TargetOS.Windows)
        {
            string path = Utility.GetWindowsLinkerPath();
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
        else if (targetOS == TargetOS.Web)
        {
            linker = "wasm-ld";
            args = $"-o \"{outputPath}\" {string.Join(" ", objectFiles)}";

            switch (type)
            {
                case AssemblyType.Application:
                    args += " --entry main";
                    break;
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
}
