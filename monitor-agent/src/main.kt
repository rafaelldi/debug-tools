import ai.koog.agents.ext.agent.simpleSingleRunAgent
import ai.koog.agents.mcp.McpToolRegistryProvider
import ai.koog.prompt.executor.llms.all.simpleOllamaAIExecutor
import ai.koog.prompt.llm.OllamaModels

suspend fun main() {
    val toolRegistry = McpToolRegistryProvider.fromTransport(
        transport = McpToolRegistryProvider.defaultSseTransport("http://localhost:5197")
    )

    val agent = simpleSingleRunAgent(
        simpleOllamaAIExecutor("http://localhost:11434"),
        " You are an assistant helping diagnose different problems with .NET applications.",
        OllamaModels.Meta.LLAMA_3_2,
        toolRegistry = toolRegistry
    )
}