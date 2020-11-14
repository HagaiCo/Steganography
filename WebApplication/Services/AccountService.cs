using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
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
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey)); 
            var a = await auth.CreateUserWithEmailAndPasswordAsync(signUpRequest.Email, signUpRequest.Password, signUpRequest.Name, true);
            var response = await _client.PushAsync("Users/", signUpRequest);

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
    }
}