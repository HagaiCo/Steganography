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
        private static string Bucket = "steganography-5582f.firebaseio.com";

        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SignUp(SignUpModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
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
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            try
            {
                // Verification.
                if (this.Request.IsAuthenticated)
                {
                    return this.RedirectToLocal(returnUrl);
                }
            }
            catch (Exception ex)
            {
                // Info
                Console.Write(ex);
            }

            // Info.
            return this.View();
        }

        // GET: Account
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                // Verification.
                if (ModelState.IsValid)
                {
                    var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                    var ab = await auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);
                    string token = ab.FirebaseToken;
                    var user = ab.User;
                    if (token != "")
                    {

                        SignInUser(user.Email, token, false);
                        return RedirectToLocal(returnUrl);

                    }
                    else
                    {
                        // Setting.
                        ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Info
                Console.Write(ex);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        private void SignInUser(string email, string token, bool isPersistent)
        {
            // Initialization.
            var claims = new List<Claim>();

            try
            {
                // Setting
                claims.Add(new Claim(ClaimTypes.Email, email));
                claims.Add(new Claim(ClaimTypes.Authentication, token));
                var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;
                // Sign In.
                authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, claimIdenties);
            }
            catch (Exception ex)
            {
                // Info
                throw ex;
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
                    return Redirect(returnUrl);
                }

                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            //info
            return this.RedirectToAction("LogOut", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult LogOut()
        {
            var ctx = Request.GetOwinContext();
            if (ctx.Authentication.User == null)
            {
                
            }
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