using LoginRegistration.DTOs;
using LoginRegistration.DTOs.ResumeDTOs;
using LoginRegistration.Services;
using Microsoft.AspNetCore.Mvc;


namespace LoginRegistration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeReaderController : ControllerBase
    {
        private readonly ResumeReaderService resumeReaderService;
        public ResumeReaderController(ResumeReaderService resumeReaderService)
        {
            this.resumeReaderService = resumeReaderService;
        }

        [HttpPost("ReadResume")]
        public async Task<ApiResponse<ResumeResponseDto>> ReadResume(IFormFile file)
        {
            var response = await resumeReaderService.ReadResume(file);
            if (response.StatusCode! == 200)
            {
                // return StatusCode(((int)response.StatusCode, response);
            }
            return response;
        }


        [HttpPost("UploadPdf")]
        public async Task<ApiResponse<Dictionary<string, List<string>>>> UploadPdfAsync(IFormFile file)
        {
            var result = await resumeReaderService.UploadPdfAsync(file);
            return result;
        }
    }
}
