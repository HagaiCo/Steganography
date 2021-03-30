using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Firebase.Auth;
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
            //todo - make sure the user view the error message so he will understand what's wrong with his password.
            if (!ValidatePassword(signUpRequest.Password, out var errorMessage))
            {
                throw new Exception(errorMessage);
            }
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey)); 
            var a = await auth.CreateUserWithEmailAndPasswordAsync(signUpRequest.Email, signUpRequest.Password, signUpRequest.Name, true);
            await _client.PushAsync("Users/", signUpRequest);
        }
        
        public async Task<FirebaseAuthLink> Login(string email, string password)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            return  result;
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

        public List<SignUpRequestModel> GetAllUsers()
        {
            List<SignUpRequestModel> listOfFileData = null;
            try
            {
                var resultAsJsonString = _client.Get("Users/").Body;
                if(resultAsJsonString == "null")
                    return null;
                
                
                dynamic data = JsonConvert.DeserializeObject<dynamic>(resultAsJsonString);
                listOfFileData = ((IDictionary<string, JToken>)data).Select(k => 
                    JsonConvert.DeserializeObject<SignUpRequestModel>(k.Value.ToString())).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return listOfFileData;
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