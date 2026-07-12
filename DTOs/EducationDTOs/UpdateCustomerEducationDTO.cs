namespace LoginRegistration.DTOs.EducationDTOs
{
    public class UpdateCustomerEducationDTO
    {
        // Customer
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string Email { get; set; }

        // Education
        public string Qualification { get; set; }
        public string University { get; set; }
        public string CollegeName { get; set; }
        public int PassingYear { get; set; }
        public decimal Percentage { get; set; }

        //public string Certificate { get; set; }
    }
}
