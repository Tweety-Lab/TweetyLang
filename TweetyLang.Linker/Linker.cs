using LLVMSharp.Interop;

namespace TweetyLang.Linker;

public static class Linker
{
    /// <summary>
    /// Converts an LLVM module to an object file.
    /// </summary>
    /// <param name="module">LLVM Module.</param>
    /// <param name="objectOutputPath">Paht to write the object file to.</param>
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
}
