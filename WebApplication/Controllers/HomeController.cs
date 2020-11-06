using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using Firebase.Auth;
using Firebase.Storage.Options;
using FireSharp;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication.Models;
using WebApplication.ResposeModel;
using WebApplication.Utilities;

namespace WebApplication.Controllers
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
        [Authorize]
        public async Task<ActionResult> Index(FileDataUploadRequestModel fileDataUploadRequestModel)
        {
            FileStream stream;
            TaskAwaiter<bool> uploadSucceed;
            if(fileDataUploadRequestModel.File.ContentLength > 0)
            {
                var path = Path.Combine(Server.MapPath("~/Content/images/"), fileDataUploadRequestModel.File.FileName);
                fileDataUploadRequestModel.FilePath = path;
                fileDataUploadRequestModel.File.SaveAs(path);
                using ( stream = new FileStream(Path.Combine(path), FileMode.Open))
                    uploadSucceed = Task.Run(() => Upload(fileDataUploadRequestModel, stream)).GetAwaiter();
            }
            if(uploadSucceed.GetResult())
                ModelState.AddModelError(string.Empty, "Uploaded Successfully");
            return View();
        }
        public async Task<bool> Upload(FileDataUploadRequestModel fileDataUploadRequestModel, FileStream stream)
        {
            GetPermittedFilesData();
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(adminEmail, adminPass);

            var cancellation = new CancellationTokenSource();
            var firebaseStorageOptions = new FirebaseStorageOptions 
            {
                AuthTokenAsync = () => Task.FromResult(a.FirebaseToken),
                ThrowOnCancel = true     
            };
            _client = new FirebaseClient(config);
            var data = fileDataUploadRequestModel;
            
            //convert FileDataUploadRequestModel object to FileDataUploadResponseModel object:
            var fileDataUploadResponseModel = data.Convert();
            
            try
            {
                var response = await _client.PushAsync("Files/", fileDataUploadResponseModel);
                fileDataUploadResponseModel.Id = response.Result.name;
                await _client.SetAsync("Files/" + fileDataUploadResponseModel.Id, fileDataUploadResponseModel);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception was thrown: {ex}");
                ModelState.AddModelError(string.Empty, ex.Message);
                throw;
            }
        }

        public List<FileDataUploadResponseModel> GetAllFilesData()
        {
            List<FileDataUploadResponseModel> listOfFileData = null;
            try
            {
                var resultAsJsonString = _client.Get("Files/").Body;
                if(resultAsJsonString == "null")
                    return null;
                
                
                dynamic data = JsonConvert.DeserializeObject<dynamic>(resultAsJsonString);
                listOfFileData = ((IDictionary<string, JToken>)data).Select(k => 
                    JsonConvert.DeserializeObject<FileDataUploadResponseModel>(k.Value.ToString())).ToList();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return listOfFileData;
        }

        public FileDataUploadResponseModel GetFileById(string id)
        {
            var allFiles = GetAllFilesData();
            var requestedFile = allFiles.SingleOrDefault(x => x.Id == id);
            return requestedFile;
        }
        
        public bool DownloadFile(string fileId)
        {
            try
            {
                var fileToDownload = GetFileById(fileId);
                string downloadPath = Environment.GetEnvironmentVariable("USERPROFILE")+@"\"+@"Downloads\";
                var pathString = Path.Combine(downloadPath, fileToDownload.FileName);
                System.IO.File.WriteAllBytes(fileToDownload.FileName, fileToDownload.File); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public List<FileDataUploadResponseModel> GetPermittedFilesData()
        {
            var requestingUserEmail = HttpContext.GetOwinContext().Authentication.User.Claims.First().Value;
            var allFilesData = GetAllFilesData();
            var permittedFilesData = allFilesData.Where(x => x.PermittedUsers.Contains(requestingUserEmail)).ToList();
            return permittedFilesData;
        }
        
        [WebMethod]
        public static IEnumerable<SelectListItem> GetAllUsers()
        {
            //todo - this is demo data, need to refactor 
            var list1 = new List<string>(){"hagai729@gmail.com"};
            var selectListItems = list1.Select(x => new SelectListItem(){ Value = x, Text = x }).ToList();

            return selectListItems.AsEnumerable();
        }
    }
}