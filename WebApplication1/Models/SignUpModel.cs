using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class SignUpModel
    {

        [Key] 
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")] 
        public string Name { get; set; }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")] 
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}