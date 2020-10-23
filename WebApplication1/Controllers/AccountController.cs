using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Firebase.Auth;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private static string ApiKey = "AIzaSyBXcxNJb-mTnFWQYshXyELXZyj14u40xwQ";
        private static string Bucket = "https://steganography-5582f.firebaseio.com/";

        public ActionResult SignUp()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> SignUp(SignUpModel model)
        {
            if (model.Id == 0)
                return View();
            try
            {
                var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                var a = await auth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password, model.Name, true);
                ModelState.AddModelError(string.Empty, "Please Verify your email then login.");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                Console.WriteLine(e);
            }
            return RedirectToAction("Login", "Account");
        }

        // GET: Account
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                var a = await auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);
                string token = a.FirebaseToken;
                var user = a.User;
                if (token != "")
                {

                    this.SignInUser(user.Email, token, false);
                    return this.RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return this.View();
        }

        private void SignInUser(string email, string token, bool isPersistent)
        {
            //Initialization
            var claim = new List<Claim>();

            try
            {
                //Setting
                claim.Add(new Claim(ClaimTypes.Email, email));
                claim.Add(new Claim(ClaimTypes.Authentication, token));
                var claimIdenties = new ClaimsIdentity(claim, DefaultAuthenticationTypes.ApplicationCookie);
                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;
                //Sign in
                authenticationManager.SignIn(new AuthenticationProperties() {IsPersistent = isPersistent}, claimIdenties);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void ClaimIdentities(string userName, string isPersistent)
        {
            //Initialization
            var claims = new List<Claim>();
            try
            {
                //Setting
                claims.Add(new Claim(ClaimTypes.Name, userName));
                var claimIdentities = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            try
            {
                //Verification
                if (Url.IsLocalUrl(returnUrl))
                {
                    //info
                    return this.Redirect(returnUrl);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            //info
            return this.RedirectToAction("LogOff", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult LogOff()
        {
            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}