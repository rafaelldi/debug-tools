import ai.koog.agents.ext.agent.simpleSingleRunAgent
import ai.koog.agents.mcp.McpToolRegistryProvider
import ai.koog.prompt.executor.clients.google.GoogleModels
import ai.koog.prompt.executor.llms.all.simpleGoogleAIExecutor

suspend fun main() {

    val toolRegistry = McpToolRegistryProvider.fromTransport(
        McpToolRegistryProvider.defaultSseTransport("http://localhost:5143")
    )

    val apiKey = System.getenv("GOOGLE_AI_API_KEY") ?: ""
    val executor = simpleGoogleAIExecutor(apiKey)

    val agent = simpleSingleRunAgent(
        executor,
        "You are a senior performance engineer helping diagnose and fix different performance problems with .NET applications.",
        GoogleModels.Gemini2_0Flash,
        toolRegistry = toolRegistry
    )

    val result = agent.runAndGetResult("Hello! Could you understand what is happening with `monitor-samples` (pid: 40360) process?")

    println(result)
}