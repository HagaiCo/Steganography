using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Firebase.Auth;
using FireSharp.Extensions;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication.RequestModel;

namespace WebApplication.Services
{
    public class AccountService : BaseService
    {
        public async Task SignUp(SignUpRequestModel signUpRequest)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            signUpRequest.Password.First().CreationTime = DateTime.Now;
            signUpRequest.Password.First().IsCurrentPassword = true;

            await auth.CreateUserWithEmailAndPasswordAsync(signUpRequest.Email, signUpRequest.Password.First().Password, signUpRequest.Name, true);
            await Client.PushAsync("InternalUsers/", signUpRequest);
        }
        
        public async Task<FirebaseAuthLink> Login(string email, string password)
        {
            //Validate password:
            var allUsers = GetAllUsers();
            if (allUsers != null)
            {
                var currentUser = allUsers.FirstOrDefault(user => user.Value.Email == email);
                var monthsAgo = DateTime.Now.AddMonths(-int.Parse(PasswordMonthTimeStamp));
                var isPasswordNeedToChange =
                    currentUser.Value.Password.First(pass => pass.IsCurrentPassword).CreationTime < monthsAgo;
                if (isPasswordNeedToChange)
                {
                    throw new Exception("Your password was expired!");
                }
            }
            //--------------------------------------------------------
            
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            return  result;
        }

        public async Task ChangePassword(string oldPassword, string newPassword)
        {
            var allUsers = GetAllUsers();
            var currentUserEmail = HttpContext.Current.GetOwinContext().Authentication.User.Claims.First().Value;
            var currentUser = allUsers.First(user => user.Value.Email == currentUserEmail);
            var oldPass = currentUser.Value.Password.First(p => p.IsCurrentPassword);
            //Validate old Password:
            if (oldPass.Password != oldPassword)
            {
                throw new Exception("Your current password is incorrect!");
            }
            
            foreach (var password in currentUser.Value.Password)
            {
                if (password.Password == newPassword)
                {
                    throw new Exception("Please use a new password!");
                }
            }

            currentUser.Value.Password.ForEach(pass => pass.IsCurrentPassword = false);
            var passObj = new PasswordData()
            {
                Password = newPassword,
                CreationTime = DateTime.Now,
                IsCurrentPassword = true
            };
            currentUser.Value.Password.Add(passObj);

            await Client.DeleteAsync($"InternalUsers/{currentUser.Key}");
            await Client.PushAsync("InternalUsers/", currentUser.Value);
            
            Client.ChangePassword(currentUser.Value.Email, oldPassword, newPassword);
        }
        
        public void SignInUser(string email, string token, bool isPersistent, IOwinContext context)
        {
            // Initialization.
            var claims = new List<Claim>();

            // Setting
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim(ClaimTypes.Authentication, token));
            var claimIdentities = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
            var authenticationManager = context.Authentication;
            // Sign In.
            authenticationManager.SignIn(new AuthenticationProperties() {IsPersistent = isPersistent}, claimIdentities);
        }

        public Dictionary<string, SignUpRequestModel> GetAllUsers()
        {
            Dictionary<string, SignUpRequestModel> dicOfFileData = null;
            try
            {
                var resultAsJsonString = Client.Get("InternalUsers/").Body;
                if (resultAsJsonString == "null")
                    return null;
                dicOfFileData = JsonConvert.DeserializeObject<Dictionary<string, SignUpRequestModel>>(resultAsJsonString);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return dicOfFileData;
        }
        
        private bool ValidatePassword(string password, out string errorMessage)
        {
            var input = password;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Password should not be empty");
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasLowerChar.IsMatch(input))
            {
                errorMessage = "Password should contain at least one lower case letter.";
                return false;
            }

            if (!hasUpperChar.IsMatch(input))
            {
                errorMessage = "Password should contain at least one upper case letter.";
                return false;
            }
            if (!hasMiniMaxChars.IsMatch(input))
            {
                errorMessage = "Password should not be lesser than 8 or greater than 15 characters.";
                return false;
            }
            if (!hasNumber.IsMatch(input))
            {
                errorMessage = "Password should contain at least one numeric value.";
                return false;
            }

            if (!hasSymbols.IsMatch(input))
            {
                errorMessage = "Password should contain at least one special case character.";
                return false;
            }
            return true;
        }
    }
}