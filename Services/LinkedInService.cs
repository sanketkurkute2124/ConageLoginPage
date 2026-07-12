using LoginRegistration.Data;
using LoginRegistration.DTOs;
using LoginRegistration.DTOs.Auth;
using LoginRegistration.DTOs.LinkedInDTOs;
using LoginRegistration.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LoginRegistration.Services
{
    public class LinkedInService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LinkedInService> _logger;

        public LinkedInService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<LinkedInService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ApiResponse<LinkedInLoginResponseDto>> LoginAsync(LinkedInLoginDto model)
        {
            try
            {               
                using var client = new HttpClient();
                var tokenRequest = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", model.Code },
                    { "client_id", _configuration["LinkedIn:ClientId"] },
                    { "client_secret", _configuration["LinkedIn:ClientSecret"] },
                    { "redirect_uri", _configuration["LinkedIn:RedirectUri"] }
                };
                var tokenResponse = await client.PostAsync(
                     _configuration["LinkedIn:TokenUrl"],
                    new FormUrlEncodedContent(tokenRequest));

                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();


                var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

                string idToken = tokenData.GetProperty("id_token").GetString();

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(idToken);

                string email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                string firstName = jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
                string lastName = jwt.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;

                // 3. Find Existing Customer

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(x =>
                        x.Email == email);

                // 4. Create New Customer

                if (customer == null)
                {
                    customer = new Customer
                    {
                        FirstName = firstName ?? "",
                        LastName = lastName ?? "",
                        Email = email,
                        Password = "",
                        PhoneNumber = "",
                        Role = "User"
                    };


                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // 5. Generate JWT

                var claims = new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        customer.Id.ToString()),

                    new Claim(
                        ClaimTypes.Name,
                        customer.FirstName ?? ""),

                    new Claim(
                        ClaimTypes.Email,
                        customer.Email ?? ""),

                    new Claim(
                        ClaimTypes.Role,
                        customer.Role ?? "User")
                };


                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                    _configuration["JwtSettings:Secret"]));


                var credentials = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

                var jwtToken = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: credentials);



                var jwttoken =
                    new JwtSecurityTokenHandler()
                    .WriteToken(jwtToken);



                var response = new LinkedInLoginResponseDto
                {
                    Token = jwttoken,
                    CustomerId = customer.Id,
                    CustomerName = customer.FirstName,
                    Email = customer.Email,
                    Role = customer.Role
                };

                return new ApiResponse<LinkedInLoginResponseDto>(
                 200,
                 response
                 );
            }
            catch (Exception ex)
            {
                return new ApiResponse<LinkedInLoginResponseDto>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }
    }
}