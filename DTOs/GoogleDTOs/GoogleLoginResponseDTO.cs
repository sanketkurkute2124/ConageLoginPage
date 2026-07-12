namespace LoginRegistration.DTOs.GoogleDTOs
{
    public class GoogleLoginResponseDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Message { get; set; }

        public string Email { get; set; }

        // JWT token and metadata
        public string Token { get; set; }
        public string Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
