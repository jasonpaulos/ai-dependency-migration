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
        Arguments = ["--project", Path.Join(rootDir, @"FileSystemMcpServer/FileSystemMcpServer.csproj")],
        Name = "Minimal MCP Server",
    }));

// List all available tools from the MCP server.
Console.WriteLine("Available tools:");
IList<McpClientTool> tools = await mcpClient.ListToolsAsync();
foreach (McpClientTool tool in tools)
{
    Console.WriteLine($"{tool}");
}
Console.WriteLine();

// Conversational loop that can utilize the tools via prompts.
List<ChatMessage> messages =  [
        new ChatMessage(ChatRole.System, """
            You are an expert programmer and dependency manager.
            You help people migrate their codebases from one dependency to another.
            If there is any ambiguity in the request, ask for clarification.
        """)
    ];
while (true)
{
    Console.Write("Prompt: ");
    messages.Add(new(ChatRole.User, Console.ReadLine()));

    List<ChatResponseUpdate> updates = [];
    await foreach (ChatResponseUpdate update in client
        .GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
    {
        // Debugging info to show function calls and results.
        foreach (var content in update.Contents)
        {
            if (content is FunctionCallContent functionCallContent)
            {
                Console.Write($">> Function call ({functionCallContent.CallId}): {functionCallContent.Name} ");
                if (functionCallContent.Arguments != null)
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
                Console.Write($">> Function result ({functionResultContent.CallId}): ");
                if (functionResultContent.Result != null)
                {
                    Console.WriteLine($"with result: {functionResultContent.Result}");
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
