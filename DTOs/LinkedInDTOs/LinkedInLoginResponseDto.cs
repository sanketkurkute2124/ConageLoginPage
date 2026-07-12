namespace LoginRegistration.DTOs.LinkedInDTOs
{
    public class LinkedInLoginResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
