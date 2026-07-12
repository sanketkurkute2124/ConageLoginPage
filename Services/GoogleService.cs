using Google.Apis.Auth;
using LoginRegistration.Data;
using LoginRegistration.DTOs;
using LoginRegistration.DTOs.GoogleDTOs;
using LoginRegistration.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginRegistration.Services
{
    public class GoogleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleService> _logger;

        public GoogleService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<GoogleService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<GoogleLoginResponseDTO>> LoginAsync(GoogleLoginDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.IdToken))
                {
                    return new ApiResponse<GoogleLoginResponseDTO>(
                        StatusCodes.Status400BadRequest,
                        "Google IdToken is required."
                    );
                }

                var clientId = _configuration["Authentication:Google:ClientId"];

                if (string.IsNullOrWhiteSpace(clientId))
                {
                    return new ApiResponse<GoogleLoginResponseDTO>(
                        StatusCodes.Status500InternalServerError,
                        "Google ClientId is not configured."
                    );
                }

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, settings);

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == payload.Email);

                if (customer == null)
                {
                    customer = new Customer
                    {
                        FirstName = payload.GivenName ?? "",
                        LastName = payload.FamilyName ?? "",
                        Email = payload.Email,
                        Password = "",
                        PhoneNumber = "",
                        Role = "User"
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                    new Claim(ClaimTypes.Name, customer.FirstName ?? ""),
                    new Claim(ClaimTypes.Email, customer.Email ?? ""),
                    new Claim(ClaimTypes.Role, customer.Role ?? "User")
                };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));

                var credentials = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:Issuer"],
                    audience: _configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: credentials);

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                var response = new GoogleLoginResponseDTO
                {
                    Token = jwt,
                    CustomerId = customer.Id,
                    CustomerName = customer.FirstName,
                    Email = customer.Email,
                    Role = customer.Role
                };

                return new ApiResponse<GoogleLoginResponseDTO>(
                    StatusCodes.Status200OK,
                    response
                );
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "Invalid Google Token");

                return new ApiResponse<GoogleLoginResponseDTO>(
                    StatusCodes.Status401Unauthorized,
                    "Invalid Google token."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Login Failed");

                return new ApiResponse<GoogleLoginResponseDTO>(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred."
                );
            }
        }
    }
}