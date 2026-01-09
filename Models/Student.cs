using System.ComponentModel.DataAnnotations;

namespace SGP_Freelancing.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Email { get; set; }
    }
}
