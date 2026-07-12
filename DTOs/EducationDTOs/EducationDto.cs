namespace LoginRegistration.DTOs.EducationDTOs
{
    public class EducationDto
    {
        public int CustomerId { get; set; }

        public string Qualification { get; set; }

        public string CollegeName { get; set; }

        public string University { get; set; }

        public int PassingYear { get; set; }

        public decimal Percentage { get; set; }

        public IFormFile Certificate { get; set; }

    }
}
