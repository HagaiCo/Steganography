using System;

namespace WebApplication.RequestModel
{
    public static class PasswordPolicy
    {
        public const string PasswordStrengthRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,10}$";
        public const string PasswordError = "The password should contain at least:\n" +
                                            "One lower case letter.\n" +
                                            "One upper case letter.\n" +
                                            "One digit.\n" +
                                            "One special character.\n" +
                                            "Passwords must be 8-10 characters in length.\n";

    }
    
    public class PasswordData
    {
        public DateTime CreationTime { get; set; }
        public string Password { get; set; }
        public bool IsCurrentPassword { get; set; }
    }
}