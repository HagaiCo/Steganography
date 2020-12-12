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
        HideAndSeekLsb _hideAndSeekLsb =new HideAndSeekLsb();

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
            
            ;
            
           
            var fileDataUploadResponseModel = fileDataUploadRequestModel.Convert();
            
            
            
            if(!IsMediaFile(fileDataUploadResponseModel.FileName))
                fileDataUploadResponseModel.File = EncryptAndHideInPicture(fileDataUploadRequestModel);
            else
            {
                fileDataUploadResponseModel.File = EncryptAndHideInVideo(fileDataUploadResponseModel);
            }
            var response = await _client.PushAsync("Files/", fileDataUploadResponseModel);
            fileDataUploadResponseModel.Id = response.Result.name;
            var setResult = await _client.SetAsync("Files/" + fileDataUploadResponseModel.Id, fileDataUploadResponseModel);
            return setResult.StatusCode == HttpStatusCode.OK;
        }

        public string Encrypt_Aes(string plainMessage)
        {
            HideAndSeekLsb hideAndSeekLsb = new HideAndSeekLsb();
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(plainMessage, aes.Key, aes.IV);

                hideAndSeekLsb.EncryptedDataToBin(encryptedData, aes.Key, aes.IV);

                return hideAndSeekLsb.EncryptedDataToBin(encryptedData, aes.Key, aes.IV);
            }
        }

        
        public string Decrypt_Aes(byte [] cypherData, byte[] key,byte[] iv)
        {
            AesAlgo aesAlgo = new AesAlgo();
            return aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);
        }

        public string ExtractMessage(string fileId)
        {
            var fileData = GetFileById(fileId);
            if (IsMediaFile(fileData.FileName))
               return ExtractMessageFromVideo(fileId);
            else
            {
               return ExtractMessageFromPicture(fileId);
            }
        }
        
        public byte[] EncryptAndHideInPicture(FileDataUploadRequestModel fileData)
        {
            HideAndSeekLsb hideAndSeekLsb = new HideAndSeekLsb();
            Bitmap bmp = (Bitmap) Image.FromStream(fileData.File.InputStream, true, false);
            hideAndSeekLsb.Hide(bmp, Encrypt_Aes(fileData.SecretMessage));
            using (var stream = new MemoryStream())
            {
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public byte[] EncryptAndHideInVideo(FileDataUploadResponseModel fileData)
        {
            HideAndSeekLsb hideAndSeekLsb = new HideAndSeekLsb();
            HideAndSeekMetaData hideAndSeekMetaData = new HideAndSeekMetaData();
            byte[] byteVideo = fileData.File;
            hideAndSeekLsb.Hide(byteVideo, Encrypt_Aes(fileData.SecretMessage));
            //hideAndSeekMetaData.hide(byteVideo,Encrypt_Aes(fileData.SecretMessage));
            return byteVideo;
            
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

        public string ExtractMessageFromPicture(string fileId)
        {
            AesAlgo aesAlgo = new AesAlgo();
            
            var fileData = GetFileById(fileId);
            var ms = new MemoryStream(fileData.File);
            var bmp = new Bitmap(ms);
            
            byte[] cypherData = _hideAndSeekLsb.Seek(bmp);
            byte[] key = _hideAndSeekLsb.ExtractKey(bmp);
            byte[] iv = _hideAndSeekLsb.ExtractIv(bmp);

            return Decrypt_Aes(cypherData, key, iv);
        }
        
        public string ExtractMessageFromVideo(string fileId)
        {
            AesAlgo aesAlgo = new AesAlgo();
            
            var data = GetFileById(fileId);
            byte[] video = data.File;
            byte[] cypherData = _hideAndSeekLsb.Seek(video);
            byte[] key = _hideAndSeekLsb.ExtractKey(video);
            byte[] iv = _hideAndSeekLsb.ExtractIv(video);
            
            return Decrypt_Aes(cypherData,key,iv);
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