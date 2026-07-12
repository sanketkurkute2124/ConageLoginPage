namespace LoginRegistration.DTOs.EducationDTOs
{
    public class EducationResponseDto
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string Qualification { get; set; } = string.Empty;

        public string CollegeName { get; set; } = string.Empty;

        public string University { get; set; } = string.Empty;

        public int PassingYear { get; set; }

        public decimal Percentage { get; set; }

        public string? CertificatePath { get; set; }
    }
}
