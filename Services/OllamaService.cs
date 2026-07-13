using System.Net.Http.Json;
using System.Text.Json;

public class OllamaService
{
    private readonly HttpClient _httpClient;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:11434");
    }

    public async Task<string> AnalyzeResume(string resumeText)
    {
        var request = new
        {
            model = "llama3.1",
            prompt = $@"
Extract the following information from this resume.

Return ONLY JSON.

{{
  ""fullName"": """",
  ""email"": """",
  ""phone"": """",
  ""skills"": [],
  ""education"": [],
  ""experience"": [],
  ""projects"": [],
  ""keywords"": []
}}

Resume:

{resumeText}",
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            request);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        return document.RootElement
                       .GetProperty("response")
                       .GetString();
    }
}