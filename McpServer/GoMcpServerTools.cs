using ModelContextProtocol.Server;
using System.ComponentModel;
using HtmlAgilityPack;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace McpServer;

[McpServerToolType]
public static class GoMcpServerTools
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

    public static string GetBaseDirectory()
    {
        return baseDirectory;
    }

    private static bool IsValidPath(string path)
    {
        // Check if the path is within the base directory.
        return path.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase);
    }

    [McpServerTool, Description("""
    Install a go package in the specified directory.
    If the package version is not specified, it will install the latest version.
    """)]
    public static void GoInstall(string directory, string packageName, string? packageVersion)
    {
        if (!IsValidPath(directory))
        {
            throw new ArgumentException("Invalid directory path.");
        }
        if (string.IsNullOrWhiteSpace(packageName))
        {
            throw new ArgumentException("Package name cannot be empty.");
        }
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The directory '{directory}' does not exist.");
        }
        string arguments = $"get {packageName}";
        if (!string.IsNullOrWhiteSpace(packageVersion))
        {
            arguments += $"@v{packageVersion}";
        }

        ProcessHelper.RunProcess("go", arguments, directory);
    }

    [McpServerTool, Description("""
    Run 'go mod tidy' in the specified directory.
    """)]
    public static void GoModTidy(string directory)
    {
        if (!IsValidPath(directory))
        {
            throw new ArgumentException("Invalid directory path.");
        }
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The directory '{directory}' does not exist.");
        }

        ProcessHelper.RunProcess("go", "mod tidy", directory);
    }

    [McpServerTool, Description("""
    Get documentation for a Go package.
    """)]
    public static string GetGoPackageDocs(string package)
    {
        if (string.IsNullOrWhiteSpace(package))
        {
            throw new ArgumentException("Package name cannot be empty.");
        }

        var url = $"https://pkg.go.dev/{package}";

        var web = new HtmlWeb();
        var doc = web.Load(url);
        if (doc.DocumentNode == null)
        {
            throw new InvalidOperationException("Failed to load the package documentation.");
        }

        var divNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'UnitDoc')]");
        if (divNode == null)
        {
            throw new InvalidOperationException("Documentation not found in the package page.");
        }

        var text = ExtractText(divNode);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("No documentation text found.");
        }

        return text;
    }

    private static string ExtractText(HtmlNode node)
    {
        var chunks = new List<string>(); 

        foreach (var item in node.DescendantsAndSelf())
        {
            if (item.NodeType == HtmlNodeType.Text)
            {
                if (item.InnerText.Trim() != "")
                {
                    chunks.Add(item.InnerText.Trim());
                }
            }
        }
        return String.Join(" ", chunks);
    }
}
