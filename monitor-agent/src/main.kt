import ai.koog.agents.core.agent.AIAgent
import ai.koog.agents.core.agent.config.AIAgentConfig
import ai.koog.agents.core.dsl.builder.forwardTo
import ai.koog.agents.core.dsl.builder.strategy
import ai.koog.agents.core.dsl.extension.*
import ai.koog.agents.core.tools.ToolRegistry
import ai.koog.agents.core.tools.reflect.asTools
import ai.koog.agents.mcp.McpToolRegistryProvider
import ai.koog.prompt.dsl.Prompt
import ai.koog.prompt.executor.clients.google.GoogleModels
import ai.koog.prompt.executor.llms.all.simpleGoogleAIExecutor

suspend fun main() {
    val apiKey = System.getenv("GOOGLE_AI_API_KEY") ?: ""
    val executor = simpleGoogleAIExecutor(apiKey)

    val agentStrategy = strategy("Performance investigation") {
        val nodeCallLLM by nodeLLMRequest()
        val executeToolCall by nodeExecuteTool()
        val sendToolResult by nodeLLMSendToolResult()

        edge(nodeStart forwardTo nodeCallLLM)
        edge(nodeCallLLM forwardTo nodeFinish onAssistantMessage { true })
        edge(nodeCallLLM forwardTo executeToolCall onToolCall { true })
        edge(executeToolCall forwardTo sendToolResult)
        edge(sendToolResult forwardTo nodeFinish onAssistantMessage { true })
        edge(sendToolResult forwardTo executeToolCall onToolCall { true })
    }

    val agentConfig = AIAgentConfig(
        prompt = Prompt.build("Performance investigation") {
            system(
                """
                You are a senior performance engineer helping diagnose and fix different performance problems with .NET applications.
                
                If process ID is not provided, use the `GetProcessList` tool to obtain the list of processes and find the process ID from it.
                """.trimIndent()
            )
        },
        model = GoogleModels.Gemini2_0Flash,
        maxAgentIterations = 10
    )

    val toolRegistry = McpToolRegistryProvider.fromTransport(
        transport = McpToolRegistryProvider.defaultSseTransport("http://localhost:5143")
    )

    val agent = AIAgent(
        promptExecutor = executor,
        strategy = agentStrategy,
        agentConfig = agentConfig,
        toolRegistry = toolRegistry
    )

    val result =
        agent.runAndGetResult("Hello! Could you understand what is happening with `monitor-samples` process?")

    println(result)
}