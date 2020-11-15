using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Firebase.Auth;
using Firebase.Storage.Options;
using FireSharp;
using FireSharp.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication.Utilities;
using Microsoft.Owin.Host.SystemWeb;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using WebApplication.RequestModel;
using WebApplication.ResponseModel;

namespace WebApplication.Services
{
    public class HomeService : BaseService
    {
        private static readonly AccountService _accountService = new AccountService();

        public async Task<bool> Upload(FileDataUploadRequestModel fileDataUploadRequestModel)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AdminEmail, AdminPass);

            var cancellation = new CancellationTokenSource();
            var firebaseStorageOptions = new FirebaseStorageOptions 
            {
                AuthTokenAsync = () => Task.FromResult(a.FirebaseToken),
                ThrowOnCancel = true     
            };
            _client = new FirebaseClient(Config);
            var data = fileDataUploadRequestModel;
            
            //convert FileDataUploadRequestModel object to FileDataUploadResponseModel object:
            var fileDataUploadResponseModel = data.Convert();
            
            var response = await _client.PushAsync("Files/", fileDataUploadResponseModel);
            fileDataUploadResponseModel.Id = response.Result.name;
            var setResult = await _client.SetAsync("Files/" + fileDataUploadResponseModel.Id, fileDataUploadResponseModel);
            return setResult.StatusCode == HttpStatusCode.OK;
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
        
        public void DownloadFile(string fileId)
        {
            if (fileId == null) return;
            var fileToDownload = GetFileById(fileId);
            var downloadPath = Environment.GetEnvironmentVariable("USERPROFILE")+@"\"+@"Downloads\";
            var pathString = Path.Combine(downloadPath, fileToDownload.FileName);
            File.WriteAllBytes(pathString, fileToDownload.File);
        }

        public List<FileDataUploadResponseModel> GetPermittedFilesData()
        {
            var requestingUserEmail = HttpContext.Current.GetOwinContext().Authentication.User.Claims.First().Value;
            var allFilesData = GetAllFilesData();
            var permittedFilesData = allFilesData?.Where(x => x.PermittedUsers.Contains(requestingUserEmail)).ToList();
            return permittedFilesData;
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

        public void DeleteFileData(string fileId)
        {
            if (fileId == null) return;
            var resultAsJsonString = _client.DeleteAsync("Files/" + fileId);
        }
    }
}