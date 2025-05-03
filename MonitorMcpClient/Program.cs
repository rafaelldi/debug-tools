using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

Console.WriteLine("Hello, World!");

var clientTransport = new SseClientTransport(new SseClientTransportOptions
{
    Name = "Monitor Server",
    Endpoint = new Uri("http://localhost:5213/sse")
});

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"Connected to server with tools: {tool.Name}");
}

var options = new ChatOptions
{
    Tools = [.. tools]
};

using var factory = LoggerFactory.Create(builder =>
    builder
        .AddConsole()
        // .SetMinimumLevel(LogLevel.Trace)
        .SetMinimumLevel(LogLevel.Information)
);

var ollamaChatClient = new OllamaChatClient(new Uri("http://localhost:11434/"), "llama3.2:3b");
var client = new ChatClientBuilder(ollamaChatClient)
    .UseLogging(factory)
    .UseFunctionInvocation()
    .Build();

var userMessage = "Analyze the `65113` - `MonitorSamples` process problems.";

var messages = new List<ChatMessage>
{
    new(ChatRole.System, """
                         You are an assistant helping diagnose different problems with .NET applications. 
                         """),
    new(ChatRole.User, userMessage)
};

var response = await client.GetResponseAsync(messages, options);

Console.WriteLine(response);