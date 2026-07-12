using LoginRegistration.DTOs.GoogleDTOs;
using LoginRegistration.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegistration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleController : ControllerBase
    {
        private readonly GoogleService _googleService;

        public GoogleController(GoogleService googleService)
        {
            _googleService = googleService;
        }

        [HttpPost("LoginIn")]
        public async Task<IActionResult> LoginIn([FromBody] GoogleLoginDto model)
        {
            var response = await _googleService.LoginAsync(model);

            return StatusCode(response.StatusCode, response);
        }
    }
}