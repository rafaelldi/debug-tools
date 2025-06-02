import ai.koog.agents.core.tools.annotations.LLMDescription
import ai.koog.agents.core.tools.annotations.Tool
import ai.koog.agents.core.tools.reflect.ToolSet
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.engine.cio.CIO
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.request.get
import io.ktor.serialization.kotlinx.json.json
import kotlinx.serialization.Serializable

@LLMDescription("Tools for working with processes")
internal class ProcessTools : ToolSet {
    private val client = HttpClient(CIO) {
        install(ContentNegotiation) {
            json()
        }
    }

    @Tool
    @LLMDescription("Get the process list of the current machine.")
    suspend fun getProcessList(): List<ProcessInfoDto> =
        client.get("http://localhost:5143/processes").body()
}

@Serializable
internal data class ProcessInfoDto(val processId: Int, val processName: String)