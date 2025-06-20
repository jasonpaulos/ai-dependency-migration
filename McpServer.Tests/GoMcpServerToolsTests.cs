using Xunit;

namespace McpServer.Tests;

public class GoMcpServerToolsTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _originalBaseDirectory;

    public GoMcpServerToolsTests()
    {
        // Store the original base directory
        _originalBaseDirectory = GoMcpServerTools.GetBaseDirectory();

        // Create a temporary test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), "GoMcpServerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        // Set the base directory for testing
        GoMcpServerTools.SetBaseDirectoryForTesting(_testDirectory);
    }

    [Fact]
    public void GetGoPackageDocs_Test()
    {
        GoMcpServerTools.GetGoPackageDocs("github.com/cockroachdb/pebble/v2/bloom");
    }

    public void Dispose()
    {
        // Restore the original base directory
        GoMcpServerTools.SetBaseDirectoryForTesting(_originalBaseDirectory);

        // Clean up the test directory
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }
}
