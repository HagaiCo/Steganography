using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
using System.Web.Compilation;
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
        AesAlgo _aesAlgo= new AesAlgo(); 
        HideAndSeek _hideAndSeek =new HideAndSeek();

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
            // todo: encrypt and hide text in file
            if(!IsMediaFile(fileDataUploadResponseModel.FileName))
                fileDataUploadResponseModel.File = EncryptAndHide(data);
            
            var response = await _client.PushAsync("Files/", fileDataUploadResponseModel);
            fileDataUploadResponseModel.Id = response.Result.name;
            var setResult = await _client.SetAsync("Files/" + fileDataUploadResponseModel.Id, fileDataUploadResponseModel);
            return setResult.StatusCode == HttpStatusCode.OK;
        }

        public byte[] EncryptAndHide(FileDataUploadRequestModel data)
        {
            
            HideAndSeek _hideAndSeek = new HideAndSeek();
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                Bitmap bmp = (Bitmap) Image.FromStream(data.File.InputStream, true, false);
                var message = data.TextToHide;
                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(message, aes.Key, aes.IV);
                var binMessage = _hideAndSeek.EncryptedDataToBin(encryptedData, aes.Key, aes.IV);
                _hideAndSeek.Clean(bmp, binMessage.Length);
                _hideAndSeek.Hide(bmp, binMessage);

                ImageConverter converter = new ImageConverter();
                return (byte[])converter.ConvertTo(bmp, typeof(byte[]));
                
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
        
        public void DownloadFile(string fileId)
        {
            if (fileId == null) return;
            var fileToDownload = GetFileById(fileId);
            
            var downloadPath = Environment.GetEnvironmentVariable("USERPROFILE")+@"\"+@"Downloads\";
            var pathString = Path.Combine(downloadPath, fileToDownload.FileName);
            Path.GetExtension(fileToDownload.FileName);
            //todo: fileToDownload.file == > seek and decrypt
            File.WriteAllBytes(pathString, fileToDownload.File);
        }

        public string GetSecretMessage(string fileId)
        {
            AesAlgo aesAlgo = new AesAlgo();
            var data = GetFileById(fileId);
            var ms = new MemoryStream(data.File);
            var bmp = new Bitmap(ms);
            byte[] cypherData = _hideAndSeek.Seek(bmp);
            byte[] key = _hideAndSeek.ExtractKey(bmp);
            byte[] iv = _hideAndSeek.ExtractIv(bmp);

            var decryptedMessage = aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);

            return decryptedMessage;
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
        
        static string[] mediaExtensions = 
        {
            ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", //etc
            ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", //etc
            ".AVI", ".MP4", ".DIVX", ".WMV", //etc
        };
        
        public bool IsMediaFile(string fileName)
        {
            return mediaExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);
        }
    }
}