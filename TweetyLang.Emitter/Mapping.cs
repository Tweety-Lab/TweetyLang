using LLVMSharp.Interop;

namespace TweetyLang.Emitter;

/// <summary>
/// Handles conversion of TweetyLang to LLVM.
/// </summary>
internal static class Mapping
{
    /// <summary>
    /// Maps a TweetyLang type to an LLVM type.
    /// </summary>
    /// <param name="t">TweetyLang type.</param>
    /// <returns>LLVM type.</returns>
    /// <exception cref="System.NotSupportedException">Thrown when an unsupported type is encountered.</exception>
    public static LLVMTypeRef MapType(string t) => t switch
    {
        "i32" => LLVMTypeRef.Int32,
        "bool" => LLVMTypeRef.Int1,
        "void" => LLVMTypeRef.Void,
        _ => throw new System.NotSupportedException($"Unknown type {t}")
    };
}
