﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly HomeService _homeService = new HomeService();
        private static readonly AccountService _accountService = new AccountService();

        public ActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public async Task<ViewResult> Upload(FileDataUploadRequestModel fileDataUploadRequestModel)
        {
            try
            {
                FileStream stream;
                bool uploadSucceed;
                if (fileDataUploadRequestModel.File.ContentLength > 0)
                {
                    var path = Path.Combine(Server.MapPath("~/Content/images/"),
                        fileDataUploadRequestModel.File.FileName);
                    fileDataUploadRequestModel.FilePath = path;
                    fileDataUploadRequestModel.File.SaveAs(path);
                    
                    uploadSucceed = await _homeService.Upload(fileDataUploadRequestModel);
                    

                    if (uploadSucceed)
                        ModelState.AddModelError(string.Empty, "Uploaded Successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception was thrown: {ex}");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
            return View();
        }

        
        /// <summary>
        /// This method used to get all existing users to allow current user to choose with who he want to share his data.
        /// </summary>            
        public static IEnumerable<SelectListItem> GetAllUsers()
        {
            var allUsers = _accountService.GetAllUsers();
            var emailsList = allUsers.Select(x => x.Email).ToList();
            var selectListItems = emailsList.Select(x => new SelectListItem(){ Value = x, Text = x }).ToList();

            return selectListItems.AsEnumerable();
        }
    }
}