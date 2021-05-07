using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.RequestModel
{
    public class ChangePasswordRequest
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }
        
        [Required]
        [RegularExpression(PasswordPolicy.PasswordStrengthRegex, ErrorMessage = PasswordPolicy.PasswordError)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
    }
}