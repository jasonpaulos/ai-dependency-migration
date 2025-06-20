using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
string endpoint = config["AZURE_OPENAI_ENDPOINT"]!;
string apiKey = config["AZURE_OPENAI_API_KEY"]!;
string deploymentName = "gpt-4.1";

// Create an IChatClient using Azure OpenAI.
IChatClient client =
    new ChatClientBuilder(
        new AzureOpenAIClient(new Uri(endpoint),
        new AzureKeyCredential(apiKey))
        .GetChatClient(deploymentName).AsIChatClient())
    .UseFunctionInvocation()
    .Build();

var rootDir = Path.GetDirectoryName(
    Path.GetDirectoryName(
        Path.GetDirectoryName(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    AppContext.BaseDirectory)))));

// Create the MCP client
// Configure it to start and connect to your MCP server.
IMcpClient mcpClient = await McpClientFactory.CreateAsync(
    new StdioClientTransport(new()
    {
        Command = "dotnet run",
        Arguments = ["--project", Path.Join(rootDir, @"McpServer/McpServer.csproj")],
        Name = "Monolith MCP Server",
    }));

// List all available tools from the MCP server.
Console.WriteLine("Available tools:");
IList<McpClientTool> tools = await mcpClient.ListToolsAsync();
foreach (McpClientTool tool in tools)
{
    Console.WriteLine($"{tool}");
}
Console.WriteLine();

// Update my go project to use the package github.com/seiflotfy/cuckoofilter instead of github.com/bits-and-blooms/bloom/v3

// Conversational loop that can utilize the tools via prompts.
List<ChatMessage> messages =  [
        new ChatMessage(ChatRole.System, """
            You are an expert Go programmer and dependency manager.
            Your mission is to help people migrate their Go codebases from one dependency to another.
            Even when libraries are not directly compatible, you will try your best find a way to migrate the codebase, even if there are caveats or workarounds needed.
            When you are asked to migrate a dependency, you will use the available tools to read the documentation of the old and new dependencies.
            You will then analyze the codebase to find all usages of the old dependency.
            You will then determine how to replace the old dependency with the new one.
            You will change the codebase to use the new dependency, directly modifying the code files as needed.
            Don't directly modify go.mod or go.sum files, but instead use the MCP server tools to update them (GoInstall and GoModTidy).
            You will ensure all tests pass after the migration. If they do not, you will make the necessary code changes to ensure they pass with the new dependency.
            When you are done, you will create a new file called 'migration_summary.md' that contains a summary of the changes you made.
            You will then output a single message to the user, "Migration complete. See migration_summary.md for details.", or "Migration not possible" followed by a brief explanation if you were unable to migrate the codebase.
        """)
    ];

Console.Write("Prompt: ");
messages.Add(new(ChatRole.User, Console.ReadLine()));

while (true)
{
    if (messages.Last().Role == ChatRole.Assistant)
    {
        break;
    }

    List<ChatResponseUpdate> updates = [];
    await foreach (ChatResponseUpdate update in client
        .GetStreamingResponseAsync(messages, new() { Tools = [.. tools], AllowMultipleToolCalls = true }))
    {
        // Debugging info to show function calls and results.
        foreach (var content in update.Contents)
        {
            if (content is FunctionCallContent functionCallContent)
            {
                Console.Write($">> Function call ({functionCallContent.CallId}): {functionCallContent.Name} ");
                if (functionCallContent.Arguments != null && functionCallContent.Arguments.Count > 0)
                {
                    Console.WriteLine("with arguments:");
                    foreach (var arg in functionCallContent.Arguments)
                    {
                        Console.WriteLine($"  {arg.Key}: {arg.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("with no arguments.");
                }
            }
            else if (content is FunctionResultContent functionResultContent)
            {
                Console.Write($">> Function call complete ({functionResultContent.CallId}): ");
                if (functionResultContent.Result != null)
                {
                    //Console.WriteLine($"with result: {functionResultContent.Result}");
                    Console.WriteLine("with result");
                }
                else if (functionResultContent.Exception != null)
                {
                    Console.WriteLine($"with error: {functionResultContent.Exception}");
                }
                else
                {
                    Console.WriteLine("with no result.");
                }
            }
        }
        Console.Write(update);
        updates.Add(update);
    }
    Console.WriteLine();

    messages.AddMessages(updates);
}

Console.WriteLine("Exiting");
