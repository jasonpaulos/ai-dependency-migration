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
    private static readonly string baseDirectory = Path.Join(Path.GetDirectoryName(Directory.GetCurrentDirectory()), "example");

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
}
