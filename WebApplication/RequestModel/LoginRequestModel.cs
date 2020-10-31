using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models
{
    public class LoginRequestModel
    {
        [Key] 
        public int Id { get; set; }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")] 
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}