using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

// Create a generic host builder for
// dependency injection, logging, and configuration.
var builder = Host.CreateApplicationBuilder(args);

// Configure logging for better integration with MCP clients.
builder.Logging.AddConsole(consoleLogOptions =>
{
consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register the MCP server and configure it to use stdio transport.
// Scan the assembly for tool definitions.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Build and run the host. This starts the MCP server.
await builder.Build().RunAsync();

// Define a static class to hold MCP tools.
[McpServerToolType]
public static class FileSystemMcpServerTools
{
    private static string baseDirectory = Path.Join(Path.GetDirectoryName(Directory.GetCurrentDirectory()), "example");

    /// <summary>
    /// Sets the base directory for testing purposes.
    /// This method should only be used in test code.
    /// </summary>
    /// <param name="directory">The new base directory to use</param>
    public static void SetBaseDirectoryForTesting(string directory)
    {
        baseDirectory = directory;
    }

    private static bool IsValidPath(string path)
    {
        // Check if the path is within the base directory.
        return path.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase);
    }


    [McpServerTool, Description("""
    Returns the base directory which contains all relevant files.
    This is also known as the current working directory.
    No file operations outside of this directory are permitted.
    """)]
    public static string GetBaseDirectory()
    {
        return baseDirectory;
    }

    [McpServerTool, Description("Returns a list of files in the specified directory.")]
    public static string[] ListFilesInDirectory(string directory)
    {
        if (!IsValidPath(directory))
        {
            throw new ArgumentException("Invalid directory path.");
        }
        return Directory.GetFiles(directory);
    }

    [McpServerTool, Description("Reads the contents of a file.")]
    public static string ReadFile(string filePath)
    {
        if (!IsValidPath(filePath))
        {
            throw new ArgumentException("Invalid file path.");
        }
        return File.ReadAllText(filePath);
    }

    [McpServerTool, Description("""
    Writes content to a file.
    This will completely overwrite the contents of the file, if there was any.
    If the file does not exist, it will be created, along with any necessary directories.
    """)]
    public static void WriteFile(string filePath, string content)
    {
        if (!IsValidPath(filePath))
        {
            throw new ArgumentException("Invalid file path.");
        }
        // Ensure the directory exists before writing the file.
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, content);
    }

    [McpServerTool, Description("""
    Insert the given content lines into a file at a specific line range.
    The content will replace the existing lines in the specified range.
    The line range is 1-based, meaning lineStart = 1 refers to the first line.
    The end line is exclusive, meaning lineStart = 1 and lineEnd = 2 will replace the first line only.
    The file must exist, and the specified range must be valid.
    lineEnd must not exceed the number of lines already in the file, however setting lineEnd to -1 will be treated as the end of the file.
    """)]
    public static void WriteFileLines(string filePath, string[] contentLines, int lineStart, int lineEnd)
    {
        if (!IsValidPath(filePath) || !File.Exists(filePath))
        {
            throw new ArgumentException("Invalid file path.");
        }
        if (lineStart < 1 || (lineEnd < lineStart && lineEnd != -1))
        {
            throw new ArgumentException("Invalid line range specified.");
        }

        // Read the existing lines from the file.
        string[] lines = File.ReadAllLines(filePath);

        if (lineEnd == -1)
        {
            // If lineEnd is -1, set it to the end of the file.
            lineEnd = lines.Length + 1; // This will effectively append to the end.
        }

        if (lineEnd > lines.Length + 1)
        {
            throw new ArgumentException("lineEnd exceeds the number of lines in the file.");
        }

        // Replace the specified range of lines with the new content.
        List<string> updatedLines = [];
        for (int i = 0; i < lineStart - 1; i++)
        {
            updatedLines.Add(lines[i]);
        }

        updatedLines.AddRange(contentLines);

        for (int i = lineEnd - 1; i < lines.Length; i++)
        {
            updatedLines.Add(lines[i]);
        }

        // Write the updated lines back to the file.
        File.WriteAllLines(filePath, updatedLines);
    }
}
