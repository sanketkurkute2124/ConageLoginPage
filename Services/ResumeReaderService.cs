using LoginRegistration.DTOs;
using LoginRegistration.DTOs.Auth;
using LoginRegistration.DTOs.ResumeDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegistration.Services
{
    public class ResumeReaderService
    {
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

           var response=new ResumeResponseDto
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
    }


}
