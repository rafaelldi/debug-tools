import ai.koog.agents.core.tools.annotations.LLMDescription
import ai.koog.agents.core.tools.annotations.Tool
import ai.koog.agents.core.tools.reflect.ToolSet
import io.ktor.client.*
import io.ktor.client.call.*
import io.ktor.client.engine.cio.*
import io.ktor.client.plugins.contentnegotiation.*
import io.ktor.client.request.*
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json

@LLMDescription("Tools for working with processes")
internal class ProcessTools : ToolSet {
    private val client = HttpClient(CIO) {
        install(ContentNegotiation) {
            Json {
                ignoreUnknownKeys = true
            }
        }
    }

    @Tool
    @LLMDescription("Get the process list of the current machine")
    suspend fun getProcessList(): String {
        val processes: List<ProcessInfoDto> = client.get("http://localhost:5197/processes").body()
        return buildString {
            processes.forEach { appendLine("${it.processId} - ${it.processName}") }
        }
    }
}

@Serializable
internal data class ProcessInfoDto(val processId: Int, val processName: String)