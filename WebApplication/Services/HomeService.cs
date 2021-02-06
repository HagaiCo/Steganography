﻿using System;
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
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using WebApplication.RequestModel;
using WebApplication.ResponseModel;
using EncryptionMethod = WebApplication.ResponseModel.EncryptionMethod;
using HidingMethod = WebApplication.ResponseModel.HidingMethod;

namespace WebApplication.Services
{
    public class HomeService : BaseService
    {
        private static readonly AccountService _accountService = new AccountService();
        AesAlgo _aesAlgo= new AesAlgo();
        Decoder _decoder = new Decoder();
        LsbPicture _lsbPicture = new LsbPicture();
        LsbVideo _lsbVideo = new LsbVideo();
        LsbAudio _lsbAudio = new LsbAudio();
        MetaDataVideo _metaDataVideo = new MetaDataVideo();
        MetaDataAudio _metaDataAudio = new MetaDataAudio();
        MetaDataPicture _metaDataPicture = new MetaDataPicture();
        MetaDataExe _metaDataExe = new MetaDataExe();
        private LsbExe _lsbExe = new LsbExe();
        

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
            
            var fileDataUploadResponseModel = data.Convert();
            SetFileType(fileDataUploadResponseModel);

            Console.WriteLine(fileDataUploadRequestModel.HidingMethod);
            Console.WriteLine(fileDataUploadRequestModel.EncryptionMethod);
            switch (fileDataUploadResponseModel.FileType)
            {
                case FileType.Image:
                    
                    fileDataUploadResponseModel.File = EncryptAndHideInPicture(data);
                    break;
                case FileType.Video:
                    fileDataUploadResponseModel.File = EncryptAndHideInVideo(fileDataUploadResponseModel);
                    break;
                case FileType.Audio:
                    fileDataUploadResponseModel.File = EncryptAndHideInAudio(fileDataUploadResponseModel);
                    break;
                case FileType.Executable:
                    fileDataUploadResponseModel.File = EncryptAndHideInBatchFile(fileDataUploadResponseModel);
                    break;
                
            }
            
            var response = await _client.PushAsync("Files/", fileDataUploadResponseModel);
            fileDataUploadResponseModel.Id = response.Result.name;
            var setResult = await _client.SetAsync("Files/" + fileDataUploadResponseModel.Id, fileDataUploadResponseModel);
            return setResult.StatusCode == HttpStatusCode.OK;
        }
        
        /// ****************************************************************************
        /// Encryption and Decryption Algorithms 
        /// ****************************************************************************
        
        public byte[] Encrypt_Serpent(string plainMessage)
        {
            CipherKeyGenerator cipherKeyGenerator = new CipherKeyGenerator();
            cipherKeyGenerator.Init(new KeyGenerationParameters(new SecureRandom(),128 ));
            byte[] key = cipherKeyGenerator.GenerateKey();
            byte [] iv = cipherKeyGenerator.GenerateKey();
            byte[] encrptedDAta = SerpentAlgo.SerpentEncryption(plainMessage, key);
            return encrptedDAta.Concat(key).Concat(iv).ToArray();
        }
        
        public string Decrypt_Serpent(byte[] cypherData, byte[] key)
        {
            string plainText = SerpentAlgo.SerpentDecryption(cypherData, key);
            return plainText;
        }
        
        public byte[] Encrypt_Aes(string plainMessage)
        {
            
            AesAlgo aesAlgo = new AesAlgo();
            using (AesManaged aes = new AesManaged())
            {
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                byte[] encryptedData = aesAlgo.EncryptStringToBytes_Aes(plainMessage, aes.Key, aes.IV).Concat(aes.Key)
                    .Concat(aes.IV).ToArray();
                return encryptedData;

            }
        }
        
        public string Decrypt_Aes(byte [] cypherData, byte[] key,byte[] iv)
        {
            AesAlgo aesAlgo = new AesAlgo();
            return aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);
        }

        /// ****************************************************************************
        /// Encryption and hiding handlers by File type
        /// ****************************************************************************
        
        public byte[] EncryptAndHideInPicture(FileDataUploadRequestModel fileData)
        {
            Bitmap bmp = (Bitmap) Image.FromStream(fileData.File.InputStream, true, false);
            byte[] encryptedData = null;
            string encryptedBinary =null;
            
            switch (fileData.EncryptionMethod)
            {
                case EncryptionMethod.Aes: 
                    encryptedData = Encrypt_Aes(fileData.SecretMessage);
                    break;
                case EncryptionMethod.Serpent:
                    //encryptedData = Encrypt_Des(fileData.SecretMessage);
                    ////*****************
                    encryptedData = Encrypt_Serpent(fileData.SecretMessage);
                    break;
            }

            switch (fileData.HidingMethod)
            {
                case HidingMethod.Lsb:
                    encryptedBinary = _decoder.EncryptedByteArrayToBinary(encryptedData);
                    _lsbPicture.HideBitmap(bmp,encryptedBinary);
                    break;
                case HidingMethod.MetaData:
                    return _metaDataPicture.HideJpeg(File.ReadAllBytes(fileData.FilePath), encryptedData); //(File.ReadAllBytes(fileData.FilePath) = byte [] of jpeg)
                    break;
            }
            
            using (var stream = new MemoryStream())
            {
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public byte[] EncryptAndHideInVideo(FileDataUploadResponseModel fileData)
        {
            
            byte[] byteVideo = fileData.File;
            byte[] encryptedData = null;
            string encryptedBinary =null;
            
            switch (fileData.EncryptionMethod)
            {
                case EncryptionMethod.Aes:
                    encryptedData = Encrypt_Aes(fileData.SecretMessage);
                    break;
                case EncryptionMethod.Serpent:
                    //encryptedData = Encrypt_Des(fileData.SecretMessage);
                    encryptedData = Encrypt_Serpent(fileData.SecretMessage);
                    break;
            }


            switch (fileData.HidingMethod)
            {
                case HidingMethod.Lsb:
                    encryptedBinary = _decoder.EncryptedByteArrayToBinary(encryptedData);
                    if(fileData.FileExtension==".avi")
                        _lsbVideo.HideAvi(byteVideo,encryptedBinary);
                    else if (fileData.FileExtension==".mov")
                        _lsbVideo.HideMov(byteVideo,encryptedBinary);
                    break;
                case HidingMethod.MetaData:
                    if(fileData.FileExtension==".avi")
                        _metaDataVideo.hideAvi(byteVideo,encryptedData);
                    else if (fileData.FileExtension==".mov")
                        _metaDataVideo.HideMov(byteVideo,encryptedData);
                    break;
                
            }
            return byteVideo;
            
        }
        
        public byte[] EncryptAndHideInAudio(FileDataUploadResponseModel fileData)
        {
            
            byte[] byteAudio = fileData.File;
            byte[] encrypteMessage = null;
            string encryptedBinary =null;
            
            switch (fileData.EncryptionMethod)
            {
                case EncryptionMethod.Aes:
                    encrypteMessage = Encrypt_Aes(fileData.SecretMessage);
                    break;
                case EncryptionMethod.Serpent:
                    encrypteMessage = Encrypt_Serpent(fileData.SecretMessage);
                    break;
            }


            switch (fileData.HidingMethod)
            {
                case HidingMethod.Lsb:
                    encryptedBinary = _decoder.EncryptedByteArrayToBinary(encrypteMessage);
                    if(fileData.FileExtension==".wav")
                        _lsbAudio.HideWave(byteAudio,encryptedBinary);
                    else if (fileData.FileExtension == ".mp3")
                    {
                        byteAudio = _lsbAudio.GenerateFramesMp3(byteAudio);
                        _lsbAudio.HideMp3(byteAudio, encryptedBinary);
                    }
                    break;
                case HidingMethod.MetaData:
                    if(fileData.FileExtension==".wav")
                        _metaDataAudio.HideWave(byteAudio,encrypteMessage);
                    else if (fileData.FileExtension == ".mp3")
                    {
                        byteAudio = _metaDataAudio.GenerateFramesMp3(byteAudio);
                        _metaDataAudio.HideMp3(byteAudio, encrypteMessage);
                    }
                    break;
            }
            return byteAudio;
        }
        
        public byte[] EncryptAndHideInBatchFile(FileDataUploadResponseModel fileData)
        {
            
            byte[] file = fileData.File;
            byte[] encrypteMessage = null;
            string encryptedBinary =null;
            
            switch (fileData.EncryptionMethod)
            {
                case EncryptionMethod.Aes:
                    encrypteMessage = Encrypt_Aes(fileData.SecretMessage);
                    break;
                case EncryptionMethod.Serpent:
                    encrypteMessage =Encrypt_Serpent(fileData.SecretMessage);
                    break;
            }

            switch (fileData.HidingMethod)
            {
                case HidingMethod.Lsb:
                    encryptedBinary = _decoder.EncryptedByteArrayToBinary(encrypteMessage);
                    if (fileData.FileExtension == ".exe")
                    {
                        _lsbExe.HidePE(file,encryptedBinary);
                    }
                    
                    break;
                case HidingMethod.MetaData:
                    if (fileData.FileExtension == ".exe")
                    {
                        _metaDataExe.HideMetaDataPE(file, encrypteMessage);
                    }
                    else if (fileData.FileExtension == ".bat")
                    {
                        _metaDataExe.HideBatch(file,encryptedBinary);
                    }
                    break;
            }
            return file;
        }
        
        /// ****************************************************************************
        /// Decryption and Seeking handlers by File type
        /// ****************************************************************************
        
        public string ExtractMessage(string fileId)
        {
            var fileData = GetFileById(fileId);
            if (fileData.FileType == FileType.Video)
            {
                return ExtractMessageFromVideo(fileId);
            }
            if (fileData.FileType == FileType.Image)
            {
                return ExtractMessageFromPicture(fileId);
            }
            if (fileData.FileType == FileType.Audio)
            {
                return ExtractMessageFromAudio(fileId);
            }
            if (fileData.FileType == FileType.Executable)
            {
                return ExtractMessageFromExe(fileId);
            }

            return "No Suitable file type was uploaded";
        }

        public string ExtractMessageFromExe(string fileId)
        {
            AesAlgo aesAlgo = new AesAlgo();
            
            var Data = GetFileById(fileId);
            // var ms = new MemoryStream(fileData.File);
            byte[] Exe = Data.File;
            byte[] cypherData = null;
            byte[] key = null;
            byte[] iv = null;
            string decryptedMessage = null;

            switch (Data.HidingMethod)
            {
                case HidingMethod.Lsb:
                    if (Data.FileExtension == ".exe")
                    {
                        cypherData = _lsbExe.SeekPE(Exe);
                        key = _lsbExe.ExtractKeyPE(Exe);
                        iv = _lsbExe.ExtractIvPE(Exe);
                    }
                    break;
                case HidingMethod.MetaData:
                    if (Data.FileExtension == ".exe")
                    {
                        cypherData = _metaDataExe.SeekPE(Exe);
                        key = _metaDataExe.ExtractKeyPE(Exe);
                        iv = _metaDataExe.ExtractIvPE(Exe);
                    }
                    else if (Data.FileExtension == ".bat")
                    {
                        cypherData = _metaDataExe.SeekBatch(Exe);
                        key = _metaDataExe.ExtractKeyBatch(Exe);
                        iv = _metaDataExe.ExtractIvBatch(Exe);
                    }
                    break;
            }
            
            switch (Data.EncryptionMethod)
            {
                case EncryptionMethod.Aes:
                    decryptedMessage=Decrypt_Aes(cypherData,key,iv);
                    break;
                case EncryptionMethod.Serpent:
                    decryptedMessage = Decrypt_Serpent(cypherData, key);
                    break;
            }
            return decryptedMessage;
        }
        
        public string ExtractMessageFromPicture(string fileId)
        {
            AesAlgo aesAlgo = new AesAlgo();
            
            var fileData = GetFileById(fileId);
            var ms = new MemoryStream(fileData.File);
            var bmp = new Bitmap(ms);
            byte[] cypherData = null;
            byte[] key = null;
            byte[] iv = null;
            string decryptedMessage = null;
            switch (fileData.HidingMethod)
            {
                case HidingMethod.Lsb:
                    cypherData = _lsbPicture.SeekBitmap(bmp);
                    key = _lsbPicture.ExtractKeyBitmap(bmp);
                    iv = _lsbPicture.ExtractIvBitmap(bmp);
                    break;
                case HidingMethod.MetaData:
                    cypherData = _metaDataPicture.SeekJpeg(fileData.File);
                    key = _metaDataPicture.ExtractKeyJpeg(fileData.File);
                    iv = _metaDataPicture.ExtractIvJpeg(fileData.File);
                    break;
            }

            switch (fileData.EncryptionMethod)
            {
                case EncryptionMethod.Aes:
                    decryptedMessage = Decrypt_Aes(cypherData, key, iv);
                    break;
                case EncryptionMethod.Serpent:
                    decryptedMessage = Decrypt_Serpent(cypherData,key);
                    break;
            }
            return decryptedMessage;
        }
        
        public string ExtractMessageFromVideo(string fileId)
        {
            AesAlgo aesAlgo = new AesAlgo();
            
            var data = GetFileById(fileId);
            byte[] video = data.File;
            byte[] cypherData = null;
            byte[] key = null;
            byte[] iv = null;
            string decryptedMessage = null;
            
            switch (data.HidingMethod)
            {
                case HidingMethod.Lsb:
                    if (data.FileExtension == ".avi")
                    {
                        cypherData = _lsbVideo.SeekAvi(video);
                        key = _lsbVideo.ExtractKeyAvi(video);
                        iv = _lsbVideo.ExtractIvAvi(video);
                    }
                    else
                    {
                        cypherData = _lsbVideo.SeekMov(video);
                        key = _lsbVideo.ExtractKeyMov(video);
                        iv = _lsbVideo.ExtractIvMov(video);
                    }
                    break;
                case HidingMethod.MetaData:
                    if (data.FileExtension == ".avi")
                    {
                        cypherData = _metaDataVideo.SeekAvi(video);
                        key = _metaDataVideo.ExtractKeyAvi(video);
                        iv = _metaDataVideo.ExtractIvAvi(video);
                    }
                    else
                    {
                        cypherData = _metaDataVideo.SeekMov(video);
                        key = _metaDataVideo.ExtractKeyMov(video);
                        iv = _metaDataVideo.ExtractIvMov(video);
                    }
                    break;
            }

            switch (data.EncryptionMethod)
            {
                case EncryptionMethod.Aes:
                    decryptedMessage=Decrypt_Aes(cypherData,key,iv);
                    break;
                case EncryptionMethod.Serpent:
                    decryptedMessage = Decrypt_Serpent(cypherData, key);
                    break;
            }

            return decryptedMessage;
        }
        
        public string ExtractMessageFromAudio(string fileId)
        {
            AesAlgo aesAlgo = new AesAlgo();
            
            var data = GetFileById(fileId);
            byte[] audio = data.File;
            byte[] cypherData = null;
            byte[] key = null;
            byte[] iv = null;
            string decryptedMessage = null;
            
            switch (data.HidingMethod)
            {
                case HidingMethod.Lsb:
                    if (data.FileExtension == ".wav")
                    {
                        cypherData = _lsbAudio.SeekWave(audio);
                        key = _lsbAudio.ExtractKeyWave(audio);
                        iv = _lsbAudio.ExtractIvWave(audio);   
                    }
                    else
                    {
                        cypherData = _lsbAudio.SeekMp3(audio);
                        key = _lsbAudio.ExtractKeyMp3(audio);
                        iv = _lsbAudio.ExtractIvMp3(audio);
                    }
                    break;
                case HidingMethod.MetaData:
                    if (data.FileExtension == ".wav")
                    {
                        cypherData = _metaDataAudio.SeekWave(audio);
                        key = _metaDataAudio.ExtractKeyWave(audio);
                        iv = _metaDataAudio.ExtractIvWave(audio);   
                    }
                    else
                    {
                        cypherData = _metaDataAudio.SeekMp3(audio);
                        key = _metaDataAudio.ExtractKeyMp3(audio);
                        iv = _metaDataAudio.ExtractIvMp3(audio);
                    }
                    break;
            }
            switch (data.EncryptionMethod)
            {
                case EncryptionMethod.Aes:
                    decryptedMessage=Decrypt_Aes(cypherData,key,iv);
                    break;
                case EncryptionMethod.Serpent: 
                    //decryptedMessage = Decrypt_Des(cypherData, key, iv);
                    decryptedMessage = Decrypt_Serpent(cypherData, key);
                    break;
            }
            return decryptedMessage;
        }
        
        /// ****************************************************************************
        /// Server Utilities 
        /// ****************************************************************************
        
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
            /// fileToDownload.file == > seek and decrypt
            File.WriteAllBytes(pathString, fileToDownload.File);
        }
        
        public List<FileDataUploadResponseModel> GetPermittedFilesData()
        {
            var requestingUserEmail = HttpContext.Current.GetOwinContext().Authentication.User.Claims.First().Value;
            var allFilesData = GetAllFilesData();
            var permittedFilesData = allFilesData?.Where(x => x.PermittedUsers.Contains(requestingUserEmail)).ToList();
            permittedFilesData = permittedFilesData?.OrderBy(x => x.FileType).ToList();
            return permittedFilesData;
        }
        
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
        
        static string[] VideoFileExtensions = 
        {
            ".AVI", ".MP4", ".DIVX", ".WMV", //etc
            ".RMA", //etc
            ".avi", ".MP4", ".DIVX", ".WMV",".mov" //etc
        };

        static string[] AudioFileExtensions =
        {
            ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", //etc
        };
        
        static string[] ImageFileExtensions = 
        {
            ".JPEG", ".JPG", ".PNG", ".BMP", ".GIF",".tiff","TIFF"
        };
        
        static string[] ExecutableFileExtensions = 
        {
            ".EXE", ".BAT", ".APP"
        };
        
        public bool IsVideoFile(string fileName)
        {
            return VideoFileExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);
        }

        public FileDataUploadResponseModel SetFileType(FileDataUploadResponseModel fileDataUploadResponseModel)
        {
            var isVideoFile = VideoFileExtensions.Contains(Path.GetExtension(fileDataUploadResponseModel.FileName), StringComparer.OrdinalIgnoreCase);
            var isImageFile = ImageFileExtensions.Contains(Path.GetExtension(fileDataUploadResponseModel.FileName), StringComparer.OrdinalIgnoreCase);
            var isAudioFile = AudioFileExtensions.Contains(Path.GetExtension(fileDataUploadResponseModel.FileName), StringComparer.OrdinalIgnoreCase);
            var isExecutableFile = ExecutableFileExtensions.Contains(Path.GetExtension(fileDataUploadResponseModel.FileName), StringComparer.OrdinalIgnoreCase);

            if (isVideoFile)
                fileDataUploadResponseModel.FileType = FileType.Video;
            else if (isImageFile)
                fileDataUploadResponseModel.FileType = FileType.Image;
            else if (isExecutableFile)
                fileDataUploadResponseModel.FileType = FileType.Executable;
            else if (isAudioFile)
                fileDataUploadResponseModel.FileType = FileType.Audio;
            else
                fileDataUploadResponseModel.FileType = FileType.UnknownType;
            return fileDataUploadResponseModel;
        }
        
        public string GetGenericIconByFileType(FileType fileTyoe)
        {
            string fileIconName = "";
            switch (fileTyoe)
            {
                case FileType.Video:
                    fileIconName = "VideoIcon.png";
                    break;
                case FileType.Executable:
                    fileIconName = "ExecutableIcon.jpg";
                    break;
                case FileType.Audio:
                    fileIconName = "VideoIcon.png";
                    break;
            }
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileIconName);
            var fileContentInBytes = File.ReadAllBytes(path);
            return Convert.ToBase64String(fileContentInBytes);
        }
    }
}