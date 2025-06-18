using Xunit;

namespace FileSystemMcpServer.Tests
{    public class FileSystemMcpServerToolsTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _originalBaseDirectory;

        public FileSystemMcpServerToolsTests()
        {
            // Store the original base directory
            _originalBaseDirectory = FileSystemMcpServerTools.GetBaseDirectory();
            
            // Create a temporary test directory
            _testDirectory = Path.Combine(Path.GetTempPath(), "FileSystemMcpServerTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);

            // Set the base directory for testing
            FileSystemMcpServerTools.SetBaseDirectoryForTesting(_testDirectory);
        }

        [Fact]
        public void WriteFile_ShouldCreateAndWriteToFile()
        {
            // Arrange
            string testFilePath = Path.Combine(_testDirectory, "test.txt");
            string content = "Hello, World!";

            // Act
            FileSystemMcpServerTools.WriteFile(testFilePath, content);

            // Assert
            Assert.True(File.Exists(testFilePath));
            string actualContent = File.ReadAllText(testFilePath);
            Assert.Equal(content, actualContent);
        }

        [Fact]
        public void WriteFileLines_ShouldReplaceSpecifiedLines()
        {
            // Arrange - Create a fresh test file for this test
            string testFilePath = Path.Combine(_testDirectory, "test.txt");
            var initialContent = new[]
            {
                "Line 1",
                "Line 2",
                "Line 3",
                "Line 4",
                "Line 5"
            };
            File.WriteAllLines(testFilePath, initialContent);

            var newContent = new[]
            {
                "New Line A",
                "New Line B"
            };

            // Act - Replace lines 2-4 (indexes 1-3) with new content
            FileSystemMcpServerTools.WriteFileLines(testFilePath, newContent, 2, 4);

            // Assert
            var actualContent = File.ReadAllLines(testFilePath);

            var expectedContent = new[]
            {
                "Line 1",
                "New Line A",
                "New Line B",
                "Line 4",
                "Line 5"
            };
            Assert.Equal(expectedContent, actualContent);
        }

        [Fact]
        public void WriteFileLines_WithEndOfFileMinus1_ShouldReplaceToEnd()
        {
            // Arrange - Create a fresh test file for this test
            string testFilePath = Path.Combine(_testDirectory, "test.txt");
            var initialContent = new[]
            {
                "Line 1",
                "Line 2",
                "Line 3",
                "Line 4",
                "Line 5"
            };
            File.WriteAllLines(testFilePath, initialContent);

            var newContent = new[]
            {
                "New Line X",
                "New Line Y",
                "New Line Z"
            };

            // Act - Replace from line 3 to the end
            FileSystemMcpServerTools.WriteFileLines(testFilePath, newContent, 3, -1);

            // Assert
            var actualContent = File.ReadAllLines(testFilePath);

            var expectedContent = new[]
            {
                "Line 1",
                "Line 2",
                "New Line X",
                "New Line Y",
                "New Line Z"
            };
            Assert.Equal(expectedContent, actualContent);
        }

        [Fact]
        public void WriteFileLines_ShouldThrowException_WhenLineRangeIsInvalid()
        {
            // Arrange - Create a fresh test file for this test
            string testFilePath = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFilePath, "Line 1\nLine 2\nLine 3");

            // Act & Assert - Invalid start line
            var exception1 = Assert.Throws<ArgumentException>(() =>
                FileSystemMcpServerTools.WriteFileLines(testFilePath, ["Test"], 0, 2));

            Assert.Contains("Invalid line range", exception1.Message);

            // Act & Assert - Invalid end line (less than start)
            var exception2 = Assert.Throws<ArgumentException>(() =>
                FileSystemMcpServerTools.WriteFileLines(testFilePath, ["Test"], 2, 1));

            Assert.Contains("Invalid line range", exception2.Message);

            // Act & Assert - Invalid end line (beyond existing lines)
            var exception3 = Assert.Throws<ArgumentException>(() =>
                FileSystemMcpServerTools.WriteFileLines(testFilePath, ["Test"], 2, 5));

            Assert.Contains("lineEnd exceeds the number of lines in the file", exception3.Message);
        }

        public void Dispose()
        {
            // Restore the original base directory
            FileSystemMcpServerTools.SetBaseDirectoryForTesting(_originalBaseDirectory);

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
}
