
using LoginRegistration.DTOs;
using LoginRegistration.DTOs.Auth;
using LoginRegistration.DTOs.CustomerDTOs;
using LoginRegistration.Models;
using LoginRegistration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegistration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        
        [HttpGet]
        // Registers a new customer.
        [AllowAnonymous]
        [HttpPost("RegisterCustomer")]
        public async Task<ActionResult<ApiResponse<CustomerResponseDTO>>> RegisterCustomer([FromBody] CustomerRegistrationDTO customerDto)
        {
            var response = await _customerService.RegisterCustomerAsync(customerDto);
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, response);
            }
            return Ok(response);
        }

        // Logs in a customer.

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login([FromBody] LoginDTO loginDto)
        {
            var response = await _customerService.LoginAsync(loginDto);
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, response);
            }
            return Ok(response);
        }

    }
}