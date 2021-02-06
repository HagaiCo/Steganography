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
        DesAlgo _desAlgo = new DesAlgo();
        Decoder _decoder = new Decoder();
        LsbPicture _lsbPicture = new LsbPicture();
        LsbVideo _lsbVideo = new LsbVideo();
        LsbAudio _lsbAudio = new LsbAudio();
        LsbExe _lsbBatch = new LsbExe();
        MetaDataVideo _metaDataVideo = new MetaDataVideo();
        MetaDataAudio _metaDataAudio = new MetaDataAudio();
        MetaDataPicture _metaDataPicture = new MetaDataPicture();
        MetaDataExe _metaDataExe = new MetaDataExe();
        

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
        
        public byte[] Encrypt_Des(string plainMessage)
        {
            using (TripleDESCryptoServiceProvider tDes = new TripleDESCryptoServiceProvider())
            {
                tDes.KeySize = 128;
                tDes.Padding = PaddingMode.PKCS7;
                byte[] encryptedData = _desAlgo.EncryptStringToBytes_Des(plainMessage, tDes.Key, tDes.IV).Concat(tDes.Key)
                    .Concat(tDes.IV).ToArray();
                return encryptedData;

            }
        }

        
        public string Decrypt_Aes(byte [] cypherData, byte[] key,byte[] iv)
        {
            AesAlgo aesAlgo = new AesAlgo();
            return aesAlgo.DecryptStringFromBytes_Aes(cypherData, key, iv);
        }

        
        
        public string Decrypt_Des(byte [] cypherData, byte[] key,byte[] iv)
        {
            byte [] desIv = new byte[8];
            byte [] desKey = new byte[16];
            byte [] temp = new byte[8];
            Array.Copy(key,temp,8); // completing cypher data
            cypherData = cypherData.Concat(temp).ToArray();
            
            Array.Copy(key,8,desKey,0,8);// copying first 8 bytes of key from key 2nd half
            
            Array.Copy(iv,0,desKey,8,8); // copying 2nd 8 bytes of key from iv first half
            
            Array.Copy(iv,8,desIv,0,8); // copying desIv from last 8 bytes of iv
            return _desAlgo.DecryptStringFromBytes_Des(cypherData, desKey, desIv);
        }

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
                return ExtractMessageFromBatch(fileId);
            }

            return "No Suitable file type was uploaded";
        }
        
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
                    _lsbPicture.Hide(bmp,encryptedBinary);
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
                        _lsbVideo.Hide(byteVideo,encryptedBinary);
                    else if (fileData.FileExtension==".mov")
                        _lsbVideo.HideMov(byteVideo,encryptedBinary);
                    break;
                case HidingMethod.MetaData:
                    if(fileData.FileExtension==".avi")
                        _metaDataVideo.hide(byteVideo,encryptedData);
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
                    //encrypteMessage = Encrypt_Des(fileData.SecretMessage);
                    encrypteMessage = Encrypt_Serpent(fileData.SecretMessage);
                    break;
            }


            switch (fileData.HidingMethod)
            {
                case HidingMethod.Lsb:
                    encryptedBinary = _decoder.EncryptedByteArrayToBinary(encrypteMessage);
                    if(fileData.FileExtension==".wav")
                        _lsbAudio.Hide(byteAudio,encryptedBinary);
                    else if (fileData.FileExtension == ".mp3")
                    {
                        byteAudio = _lsbAudio.GenerateFrames(byteAudio);
                        _lsbAudio.HideMp3(byteAudio, encryptedBinary);
                    }
                    break;
                case HidingMethod.MetaData:
                    if(fileData.FileExtension==".wav")
                        _metaDataAudio.Hide(byteAudio,encrypteMessage);
                    else if (fileData.FileExtension == ".mp3")
                    {
                        byteAudio = _metaDataAudio.GenerateFrames(byteAudio);
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
                    //encrypteMessage = Encrypt_Des(fileData.SecretMessage);
                    encrypteMessage =Encrypt_Serpent(fileData.SecretMessage);
                    break;
            }

            switch (fileData.HidingMethod)
            {
                case HidingMethod.Lsb:
                    encryptedBinary = _decoder.EncryptedByteArrayToBinary(encrypteMessage);
                    if (fileData.FileExtension == ".exe")
                    {
                        _lsbBatch.HidePE(file,encryptedBinary);
                    }
                    else if (fileData.FileExtension == ".bat")
                    {
                        _lsbBatch.HideSecretMessage(file,encryptedBinary);
                    }
                    break;
                case HidingMethod.MetaData:
                    if (fileData.FileExtension == ".exe")
                    {
                        _metaDataExe.HideMetaDataPE(file, encrypteMessage);
                    }
                    break;
            }
            return file;
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

        public string ExtractMessageFromBatch(string fileId)
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
                        cypherData = _lsbBatch.SeekPE(Exe);
                        key = _lsbBatch.ExtractKeyPE(Exe);
                        iv = _lsbBatch.ExtractIvPE(Exe);
                    }
                    else if (Data.FileExtension == ".bat")
                    {
                        cypherData = _lsbBatch.Seek(Exe);
                        key = _lsbBatch.ExtractKey(Exe);
                        iv = _lsbBatch.ExtractIv(Exe);
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
                        cypherData = _lsbBatch.Seek(Exe);
                        key = _lsbBatch.ExtractKey(Exe);
                        iv = _lsbBatch.ExtractIv(Exe);
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
                    cypherData = _lsbPicture.Seek(bmp);
                    key = _lsbPicture.ExtractKey(bmp);
                    iv = _lsbPicture.ExtractIv(bmp);
                    break;
                case HidingMethod.MetaData:
                    cypherData = _metaDataPicture.SeekJpeg(fileData.File);
                    key = _metaDataPicture.ExtractKey(fileData.File);
                    iv = _metaDataPicture.ExtractIv(fileData.File);
                    break;
            }

            switch (fileData.EncryptionMethod)
            {
                case EncryptionMethod.Aes:
                    decryptedMessage = Decrypt_Aes(cypherData, key, iv);
                    break;
                case EncryptionMethod.Serpent:
                    //decryptedMessage = Decrypt_Des(cypherData, key, iv);
                    ////*****************
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
                        cypherData = _lsbVideo.Seek(video);
                        key = _lsbVideo.ExtractKey(video);
                        iv = _lsbVideo.ExtractIv(video);
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
                        cypherData = _metaDataVideo.Seek(video);
                        key = _metaDataVideo.ExtractKey(video);
                        iv = _metaDataVideo.ExtractIv(video);
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
                    //decryptedMessage = Decrypt_Des(cypherData, key, iv);
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
                        cypherData = _lsbAudio.Seek(audio);
                        key = _lsbAudio.ExtractKey(audio);
                        iv = _lsbAudio.ExtractIv(audio);   
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
                        cypherData = _metaDataAudio.Seek(audio);
                        key = _metaDataAudio.ExtractKey(audio);
                        iv = _metaDataAudio.ExtractIv(audio);   
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
        
        public List<FileDataUploadResponseModel> GetPermittedFilesData()
        {
            var requestingUserEmail = HttpContext.Current.GetOwinContext().Authentication.User.Claims.First().Value;
            var allFilesData = GetAllFilesData();
            var permittedFilesData = allFilesData?.Where(x => x.PermittedUsers.Contains(requestingUserEmail)).ToList();
            permittedFilesData = permittedFilesData?.OrderBy(x => x.FileType).ToList();
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