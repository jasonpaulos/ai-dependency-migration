using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer;

[McpServerToolType]
public static class FileSystemMcpServerTools
{
    private static string baseDirectory = Path.Join(Path.GetDirectoryName(
        Path.GetDirectoryName(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        AppContext.BaseDirectory))))), "example");

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

    [McpServerTool, Description("""
    Returns a list of all files and directories in the specified directory.
    The returned list contains only the names of the files and directories, not their full paths.
    Subdirectories are included in the list, with a trailing slash to indicate they are directories.
    """)]
    public static string[] ListFilesInDirectory(string directory)
    {
        if (!IsValidPath(directory))
        {
            throw new ArgumentException("Invalid directory path.");
        }
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The directory '{directory}' does not exist.");
        }
        var allFiles = Directory.GetFiles(directory).Select(file => Path.GetFileName(file));
        var allDirectories = Directory.GetDirectories(directory)
            .Select(dir => Path.GetFileName(dir) + "/");
        return [.. allFiles, .. allDirectories];
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

    // [McpServerTool, Description("""
    // Insert the given content lines into a file at a specific line range.
    // The content will replace the existing lines in the specified range.
    // The line range is 1-based, meaning lineStart = 1 refers to the first line.
    // The end line is exclusive, meaning lineStart = 1 and lineEnd = 2 will replace the first line only.
    // The file must exist, and the specified range must be valid.
    // lineEnd must not exceed the number of lines already in the file, however setting lineEnd to -1 will be treated as the end of the file.
    // """)]
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

