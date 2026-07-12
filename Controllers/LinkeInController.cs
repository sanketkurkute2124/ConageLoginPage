using LoginRegistration.DTOs.LinkedInDTOs;
using LoginRegistration.Models;
using LoginRegistration.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegistration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinkedInController : ControllerBase
    {
        private readonly LinkedInService _linkedInService;
        public LinkedInController(
            LinkedInService linkedInService)
        {      
            _linkedInService = linkedInService;
        }


        [HttpPost("LoginIn")]
        public async Task<IActionResult> Login([FromBody] LinkedInLoginDto model)
        {
            var response = await _linkedInService.LoginAsync(model);

            return StatusCode(response.StatusCode, response);
        }

    }
}

