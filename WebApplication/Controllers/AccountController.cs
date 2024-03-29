﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WebApplication.RequestModel;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public class AccountController : BaseController
    {
        private readonly AccountService _accountService = new AccountService();

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SignUp(MainRequestModel requestModel)
        {
            try
            {
                if (string.IsNullOrEmpty(requestModel.SignUpRequestModel.Email))
                    return RedirectToAction("Login", "Account");

                await _accountService.SignUp(requestModel.SignUpRequestModel);
                ModelState.AddModelError(string.Empty, "Please Verify your email then login.");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                Console.WriteLine(e);
            }
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Login(string returnUrl)
        {
            try
            {
                // Verification.
                if (Request.IsAuthenticated)
                {
                    return RedirectToLocal(returnUrl);
                }
            }
            catch (Exception ex)
            {
                // Info
                Console.Write(ex);
                ModelState.AddModelError(string.Empty, ex.Message);
                throw;
            }
            // Info.
            return View();
        }

        // GET: Account
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(MainRequestModel model, string returnUrl)
        {
            try
            {
                // Verification.
                if (ModelState.IsValid)
                {
                    var result = await _accountService.Login(model.LoginRequestModel.Email, model.LoginRequestModel.Password);
                    if (result.FirebaseToken != "")
                    {
                        _accountService.SignInUser(result.User.Email, result.FirebaseToken, false, Request.GetOwinContext());
                        return RedirectToLocal(returnUrl);
                    }
                    // Setting.
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                // Info
                Console.Write(ex);
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }
        
        public async Task<ActionResult> ChangePasswordImpl(ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                // Verification.
                if (ModelState.IsValid)
                {
                    await _accountService.ChangePassword(changePasswordRequest.OldPassword, changePasswordRequest.NewPassword);
                }
            }
            catch (Exception ex)
            {
                // Info
                Console.Write(ex);
                ModelState.AddModelError(string.Empty, "Some error occured while trying to change user password.");
            }

            return RedirectToAction("UploadFileData", "Home");
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
                    return RedirectToAction("UploadFileData", "Home");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //info
            return RedirectToAction("LogOut", "Account");
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