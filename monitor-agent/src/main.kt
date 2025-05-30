import ai.koog.agents.core.tools.ToolRegistry
import ai.koog.agents.core.tools.reflect.asTools
import ai.koog.agents.ext.agent.simpleSingleRunAgent
import ai.koog.prompt.executor.llms.all.simpleOllamaAIExecutor
import ai.koog.prompt.llm.OllamaModels

suspend fun main() {
    val toolRegistry = ToolRegistry {
        tools(ProcessTools().asTools())
    }

    val executor = simpleOllamaAIExecutor("http://localhost:11434")

    val agent = simpleSingleRunAgent(
        executor,
        "You are a senior performance engineer helping diagnose and fix different performance problems with .NET applications.",
        OllamaModels.Meta.LLAMA_3_2,
        toolRegistry = toolRegistry
    )

    val result = agent.runAndGetResult("Hello! Could you find a process id of `monitor-samples` process?")

    println(result)
}