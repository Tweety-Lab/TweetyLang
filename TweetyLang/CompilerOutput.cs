
namespace TweetyLang;

internal static class CompilerOutput
{
    /// <summary>
    /// Writes an error message.
    /// </summary>
    /// <param name="message">Error message.</param>
    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Writes an error message.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="line">Line it occurs on.</param>
    /// <param name="column">Column it occurs on.</param>
    public static void WriteError(string message, int line, int column)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"error({line}, {column}): ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    /// <summary>
    /// Writes a warning message.
    /// </summary>
    /// <param name="message">Warning message.</param>
    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Writes a warning message.
    /// </summary>
    /// <param name="message">Warning message.</param>
    /// <param name="line">Line it occurs on.</param>
    /// <param name="column">Column it occurs on.</param>
    public static void WriteWarning(string message, int line, int column)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"warning({line}, {column}): ");
        Console.ResetColor();
        Console.WriteLine(message);
    }
}
