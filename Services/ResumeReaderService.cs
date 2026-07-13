using LoginRegistration.Data;
using LoginRegistration.DTOs;
using LoginRegistration.DTOs.Auth;
using LoginRegistration.DTOs.ResumeDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;


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



     
        public async Task<ApiResponse<string>> UploadPdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }
                //return BadRequest("Please upload a PDF.");

            var apiKey = _configuration["Parseur:ApiKey"];
            var mailboxId = _configuration["Parseur:MailboxId"];

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", apiKey);

            using var form = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            form.Add(new StreamContent(stream), "file", file.FileName);

            var response = await _httpClient.PostAsync(
                $"https://api.parseur.com/parser/{mailboxId}/upload",
                form);

            var result = await response.Content.ReadAsStringAsync();

            return new ApiResponse<string>(
    StatusCodes.Status200OK,
    result
);
        }
    }


}



