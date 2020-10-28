using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using Firebase.Auth;
using Firebase.Storage.Client;
using Firebase.Storage.Options;
using FirebaseAdmin.Auth;
using FireSharp;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.FirebaseModel;
using WebApplication1.Models;
using FirebaseAuth = Firebase.Auth.FirebaseAuth;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static string ApiKey = "AIzaSyBXcxNJb-mTnFWQYshXyELXZyj14u40xwQ";
        private static string Bucket = "steganography-5582f.appspot.com";
        private static string adminEmail = "hagai729@gmail.com";
        private static string adminPass = "123123";
        static IFirebaseConfig config = new FireSharp.Config.FirebaseConfig()
        {
            AuthSecret = "jnUAPjDAutWRVPuwEO4QcRJIj1bsmZc8plnSrM9K",
            BasePath = "https://steganography-5582f.firebaseio.com/"
        };
        FirebaseClient _client = new FirebaseClient(config);

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Index(FileUploadViewModel fileUploadViewModel)
        {
            FileStream stream;
            if(fileUploadViewModel.File.ContentLength > 0)
            {
                var path = Path.Combine(Server.MapPath("~/Content/images/"), fileUploadViewModel.File.FileName);
                fileUploadViewModel.FilePath = path;
                fileUploadViewModel.File.SaveAs(path);
                using ( stream = new FileStream(Path.Combine(path), FileMode.Open));
                Task.Run(() => Upload(fileUploadViewModel, stream)).GetAwaiter();
            }
            return View();
        }
        public async void Upload(FileUploadViewModel fileUploadViewModel, FileStream stream)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(adminEmail, adminPass);

            var cancellation = new CancellationTokenSource();
            var firebaseStorageOptions = new FirebaseStorageOptions 
            {
                AuthTokenAsync = () => Task.FromResult(a.FirebaseToken),
                ThrowOnCancel = true     
            };
            _client = new FirebaseClient(config);
            var data = fileUploadViewModel;
            
            //todo - create convert method
            FileUploadFirebaseModel fileUploadFirebaseModel = new FileUploadFirebaseModel();
            fileUploadFirebaseModel.File = System.IO.File.ReadAllBytes(fileUploadViewModel.FilePath);
            fileUploadFirebaseModel.Id = data.Id;
            fileUploadFirebaseModel.FileName = data.File.FileName;
            fileUploadFirebaseModel.PermittedUsers = data.PermittedUsers;

            PushResponse response = _client.Push("Files/", fileUploadFirebaseModel);
            fileUploadFirebaseModel.Id = response.Result.name;
            SetResponse setResponse = _client.Set("Files/" + fileUploadFirebaseModel.Id, fileUploadFirebaseModel);
            //var task = new FirebaseStorage(Bucket, firebaseStorageOptions).Child("images").Child(fileUploadViewModel.File.FileName).PutAsync(stream, cancellation.Token);
        
            try
            {
                //await task;
                ModelState.AddModelError(string.Empty, "Uploaded Successfully");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception was thrown: {ex}");
            }
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
        
        [WebMethod]
        public static IEnumerable<SelectListItem> GetAllUsers()
        {
            var list1 = new List<string>(){"hagai729@gmail.com"};
            var selectListItems = list1.Select(x => new SelectListItem(){ Value = x, Text = x }).ToList();

            return selectListItems.AsEnumerable();
        }
    }
}