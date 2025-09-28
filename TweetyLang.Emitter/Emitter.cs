using LLVMSharp.Interop;
using TweetyLang.Compiler;
using TweetyLang.Parser.AST;

namespace TweetyLang.Emitter;

public static class Emitter
{
    /// <summary>
    /// Emits a single linked LLVM module for the entire compilation.
    /// </summary>
    /// <param name="compilation">Compilation.</param>
    /// <returns>Linked LLVM module.</returns>
    public static LLVMModuleRef EmitModule(TweetyLangCompilation compilation)
    {
        var irBuilder = new IRBuilder();

        // Emit all modules from all syntax trees
        var moduleMap = new Dictionary<string, LLVMModuleRef>(StringComparer.OrdinalIgnoreCase);
        var importsMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var tree in compilation.SyntaxTrees)
        {
            var program = tree.Root;
            var llvmModules = irBuilder.EmitProgram(program); // emits all modules in program

            for (int i = 0; i < program.Modules.Count; i++)
            {
                var moduleNode = program.Modules[i];
                moduleMap[moduleNode.Name] = llvmModules[i];

                // Build imports from program-level import statements, ignoring self-imports
                importsMap[moduleNode.Name] = program.Imports
                    .Where(imp => program.Modules.Any(m => m.Name == imp.ModuleName) &&
                                  !string.Equals(imp.ModuleName, moduleNode.Name, StringComparison.OrdinalIgnoreCase))
                    .Select(imp => imp.ModuleName)
                    .ToList();
            }
        }

        // Topologically sort modules based on imports (dependencies first)
        var sortedModuleNames = TopoSort(moduleMap.Keys.ToList(), importsMap);

        // Use the **first module in sorted order** as the main module
        var mainModule = moduleMap[sortedModuleNames[0]];

        // Link all remaining modules into it
        for (int i = 1; i < sortedModuleNames.Count; i++)
        {
            var srcModule = moduleMap[sortedModuleNames[i]];
            unsafe
            {
                int result = LLVM.LinkModules2(mainModule, srcModule);
                if (result != 0)
                    throw new InvalidOperationException($"Failed to link module '{sortedModuleNames[i]}'.");
            }
        }

        return mainModule;
    }

    private static List<string> TopoSort(List<string> modules, Dictionary<string, List<string>> importsMap)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tempMark = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        void Visit(string module)
        {
            if (visited.Contains(module)) return;

            if (tempMark.Contains(module))
                throw new InvalidOperationException($"Cyclic module dependency detected involving '{module}'.");

            tempMark.Add(module);

            if (importsMap.TryGetValue(module, out var deps))
            {
                foreach (var dep in deps)
                {
                    // Skip self-imports
                    if (!string.Equals(dep, module, StringComparison.OrdinalIgnoreCase))
                        Visit(dep);
                }
            }

            tempMark.Remove(module);
            visited.Add(module);

            // Add to result in dependency-first order
            result.Add(module);
        }

        foreach (var m in modules)
            Visit(m);

        return result;
    }
}
