using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Firebase.Auth;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace WebApplication.Services
{
    public class AccountService : BaseService
    {
        public async Task SignUp(string email, string password, string name)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey)); 
            var a = await auth.CreateUserWithEmailAndPasswordAsync(email, password, name, true);
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
    }
}