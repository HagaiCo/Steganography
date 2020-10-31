using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services;
using Firebase.Auth;
using Firebase.Storage.Options;
using FireSharp;
using FireSharp.Extensions;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using WebApplication.FirebaseModel;
using WebApplication.Models;
using WebApplication.ResposeModel;

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
        [AllowAnonymous]
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
            GetAllFiles();
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
            
            //todo - create convert method
            FileUploadFirebaseRequest fileUploadFirebaseRequest = new FileUploadFirebaseRequest();
            fileUploadFirebaseRequest.File = System.IO.File.ReadAllBytes(fileDataUploadRequestModel.FilePath);
            fileUploadFirebaseRequest.Id = data.Id;
            fileUploadFirebaseRequest.FileName = data.File.FileName;
            fileUploadFirebaseRequest.PermittedUsers = data.PermittedUsers;
            fileUploadFirebaseRequest.TextToHide = data.TextToHide;

            //var task = new FirebaseStorage(Bucket, firebaseStorageOptions).Child("images").Child(fileUploadViewModel.File.FileName).PutAsync(stream, cancellation.Token);
        
            try
            {
                PushResponse response = _client.Push("Files/", fileUploadFirebaseRequest);
                fileUploadFirebaseRequest.Id = response.Result.name;
                SetResponse setResponse = _client.Set("Files/" + fileUploadFirebaseRequest.Id, fileUploadFirebaseRequest);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception was thrown: {ex}");
                ModelState.AddModelError(string.Empty, ex.Message);
                throw;
            }
        }

        public List<FileDataUploadResponseModel> GetAllFiles()
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
            var allFiles = GetAllFiles();
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