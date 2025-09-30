using LLVMSharp.Interop;
using TweetyLang.AST;

namespace TweetyLang.Emitter;

/// <summary>
/// Handles conversion of TweetyLang to LLVM.
/// </summary>
internal static class Mapping
{
    /// <summary>
    /// Maps a TweetyLang type to an LLVM type.
    /// </summary>
    /// <param name="typeRef">TweetyLang type.</param>
    /// <returns>LLVM type.</returns>
    /// <exception cref="System.NotSupportedException">Thrown when an unsupported type is encountered.</exception>
    public static LLVMTypeRef MapType(TypeReference typeRef, IRBuilder builder)
    {
        LLVMTypeRef type = typeRef.BaseType switch
        {
            "i32" => LLVMTypeRef.Int32,
            "bool" => LLVMTypeRef.Int1,
            "char" => LLVMTypeRef.Int8,
            "void" => throw new InvalidOperationException("Cannot allocate 'void'"),
            _ => builder.Structs.TryGetValue(typeRef.BaseType, out var structType)
                    ? structType
                    : throw new NotSupportedException($"Unsupported type: {typeRef.BaseType}")
        };

        for (int i = 0; i < typeRef.PointerLevel; i++)
            type = LLVMTypeRef.CreatePointer(type, 0);

        return type;
    }
}
