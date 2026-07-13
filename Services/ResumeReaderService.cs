using LoginRegistration.Data;
using LoginRegistration.DTOs;
using LoginRegistration.DTOs.Auth;
using LoginRegistration.DTOs.ResumeDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Superpower.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace LoginRegistration.Services
{
    public class ResumeReaderService
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaService _ollamaService;
        private readonly IConfiguration _configuration;
        public ResumeReaderService(OllamaService ollamaService, IConfiguration configuration,HttpClient httpClient)
        {
            this._ollamaService = ollamaService;
            this._configuration = configuration;
            this._httpClient = httpClient;
        }
        public async Task<ApiResponse<ResumeResponseDto>> ReadResume(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {

            }
               // return NotFound;

            string text = "";

            if (Path.GetExtension(file.FileName).ToLower() == ".pdf")
            {
                using var stream = file.OpenReadStream();
                using var pdf = UglyToad.PdfPig.PdfDocument.Open(stream);

                foreach (var page in pdf.GetPages())
                {
                    text += page.Text + " ";
                }
            }


            

           var keywords = ExtractKeywords(text);

            // Call the OllamaService to analyze the resume text
            // string result = await _ollamaService.AnalyzeResume(text);

            var response = new ResumeResponseDto
            {
                FullText = text,
                Skills = keywords,
                Keywords = keywords
            };

            return new ApiResponse<ResumeResponseDto>(200, response);
        }

        //helper 
        private List<string> ExtractKeywords(string text)
        {
            var skills = new List<string>
    {
        "C#",
        ".NET",
        "ASP.NET",
        "ASP.NET Core",
        "MVC",
        "Web API",
        "SQL",
        "SQL Server",
        "PostgreSQL",
        "React",
        "Angular",
        "JavaScript",
        "TypeScript",
        "HTML",
        "CSS",
        "Bootstrap",
        "Azure",
        "AWS",
        "Docker",
        "Git",
        "GitHub",
        "Entity Framework",
        "EF Core",
        "LINQ",
        "REST API",
        "Microservices"
    };

            return skills
                .Where(skill => text.Contains(skill, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();
        }
        public async Task<ApiResponse<Dictionary<string, List<string>>>> UploadPdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<Dictionary<string, List<string>>>(
                    StatusCodes.Status400BadRequest,
                    "Please upload a PDF.");
            }

            var apiKey = _configuration["Parseur:ApiKey"];
            var mailboxId = _configuration["Parseur:MailboxId"];

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", apiKey);

            using var form = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            form.Add(new StreamContent(stream), "file", file.FileName);

            var uploadResponse = await _httpClient.PostAsync(
                $"https://api.parseur.com/parser/{mailboxId}/upload",
                form);

            var uploadResult = await uploadResponse.Content.ReadAsStringAsync();

            if (!uploadResponse.IsSuccessStatusCode)
            {
                return new ApiResponse<Dictionary<string, List<string>>>(
                    (int)uploadResponse.StatusCode,
                    $"Upload failed: {uploadResult}");
            }

            string? documentId;
            try
            {
                using var uploadJson = JsonDocument.Parse(uploadResult);
                var attachments = uploadJson.RootElement.GetProperty("attachments");
                if (attachments.GetArrayLength() == 0)
                {
                    return new ApiResponse<Dictionary<string, List<string>>>(
                        StatusCodes.Status422UnprocessableEntity,
                        $"No attachments returned by Parseur. Raw response: {uploadResult}");
                }
                documentId = attachments[0].GetProperty("DocumentID").GetString();
            }
            catch (Exception ex)
            {
                return new ApiResponse<Dictionary<string, List<string>>>(
                    StatusCodes.Status500InternalServerError,
                    $"Failed to parse upload response: {ex.Message}. Raw response: {uploadResult}");
            }

            var docResponse = await _httpClient.GetAsync(
                $"https://api.parseur.com/document/{documentId}");

            var documentResult = await docResponse.Content.ReadAsStringAsync();

            if (!docResponse.IsSuccessStatusCode)
            {
                return new ApiResponse<Dictionary<string, List<string>>>(
                    (int)docResponse.StatusCode,
                    $"Failed to fetch document: {documentResult}");
            }

            string content;
            try
            {
                using var doc = JsonDocument.Parse(documentResult);
                content = doc.RootElement.GetProperty("content").GetString() ?? "";
            }
            catch (Exception ex)
            {
                return new ApiResponse<Dictionary<string, List<string>>>(
                    StatusCodes.Status500InternalServerError,
                    $"Failed to parse document response: {ex.Message}. Raw response: {documentResult}");
            }

            var lines = content
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var sections = new Dictionary<string, List<string>>();
            string currentSection = "General";
            sections[currentSection] = new List<string>();

            foreach (var line in lines)
            {
                if (Regex.IsMatch(line, @"^[A-Z][A-Z\s/&-]{2,40}$"))
                {
                    currentSection = line.Trim();
                    if (!sections.ContainsKey(currentSection))
                        sections[currentSection] = new List<string>();
                    continue;
                }
                sections[currentSection].Add(line);
            }

            return new ApiResponse<Dictionary<string, List<string>>>(
                StatusCodes.Status200OK,
                sections);
        }
    }


}



