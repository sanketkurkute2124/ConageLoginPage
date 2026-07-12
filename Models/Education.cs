using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginRegistration.Models
{
    public class Education
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string Qualification { get; set; }

        public string CollegeName { get; set; }

        public string University { get; set; }

        public int PassingYear { get; set; }

        public decimal Percentage { get; set; }

        public string CertificatePath { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }
}
