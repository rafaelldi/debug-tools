import ai.koog.agents.core.tools.annotations.LLMDescription
import ai.koog.agents.core.tools.annotations.Tool
import ai.koog.agents.core.tools.reflect.ToolSet
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.engine.cio.CIO
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.request.get
import io.ktor.serialization.kotlinx.json.json

@LLMDescription("Tools for working with process thread dumps")
class ThreadDumpTools : ToolSet {
    private val client = HttpClient(CIO) {
        install(ContentNegotiation) {
            json()
        }
    }

    @Tool
    @LLMDescription("Captures and return the process thread dump.")
    suspend fun getProcessDump(
        @LLMDescription("The process id for which capture the thread dump.")
        pid: Int
    ): String = client.get("http://localhost:5143/processes/$pid/thread-dump").body()
}
