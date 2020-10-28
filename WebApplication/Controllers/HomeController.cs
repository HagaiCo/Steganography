using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static string ApiKey = "AIzaSyBXcxNJb-mTnFWQYshXyELXZyj14u40xwQ";
        private static string Bucket = "steganography-5582f.firebaseio.com";
        
        private readonly IHostingEnvironment _env;

        public HomeController(IHostingEnvironment env)
        {
            _env = env;
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(FileUploadViewModel file)
        {
            var fileUploaded = file.File;
            FileStream fs;
            if (fileUploaded.Length > 0)
            {
                //upload file to firebase
                string folderName = "firebaseFiles";
                string path = Path.Combine(_env.WebRootPath, $"Images/{folderName}");
                fs = new FileStream(Path.Combine(path, fileUploaded.FileName), FileMode.Open);
            }

            return BadRequest();
        }
        
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}