using System.ComponentModel.DataAnnotations;

namespace WebApplication.RequestModel
{
    public class SignUpRequestModel
    {

        [Key] 
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")] 
        public string Name { get; set; }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")] 
        public string Email { get; set; }

        [Required]
        // [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]    
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,10}$", 
            ErrorMessage = "The password should contain at least:\n" +
                           "One lower case letter.\n" +
                           "One upper case letter.\n" +
                           "One digit.\n" +
                           "One special character.\n" +
                           "Passwords must be 8-10 characters in length.\n")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}