namespace LoginRegistration.DTOs.Auth
{
    public class LoginResponseDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Message { get; set; }

        // JWT token and metadata
        public string Token { get; set; }
        public string Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
