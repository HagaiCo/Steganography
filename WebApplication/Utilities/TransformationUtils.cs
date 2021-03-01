using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Web;
using WebApplication.RequestModel;
using WebApplication.ResponseModel;

namespace WebApplication.Utilities
{
    public static class TransformationUtils
    {
        public static FileDataUploadResponseModel Convert(this FileDataUploadRequestModel data)
        {
            var obj = new FileDataUploadResponseModel
            {
                File = File.ReadAllBytes(data.FilePath),
                Id = data.Id,
                FileName = data.FileAsHttpPostedFileBase.FileName,
                PermittedUsers = data.PermittedUsers,
                //SecretMessage = data.SecretMessage,
                EncryptionMethod = data.EncryptionMethod,
                HidingMethod = data.HidingMethod,
                FileExtension = Path.GetExtension(data.FilePath)
            };
            return obj;
        }
        
        
        public static FileDataUploadRequestModel Convert(this FileDataUploadResponseModel data)
        {
            var obj = new FileDataUploadRequestModel
            {
                FileAsByteArray = data.File,
                Id = data.Id,
                PermittedUsers = data.PermittedUsers,
                //SecretMessage = data.SecretMessage,
                EncryptionMethod = data.EncryptionMethod,
                HidingMethod = data.HidingMethod,
                FileType = data.FileType,
                FileName = data.FileName
            };
            return obj;
        }
        public static Bitmap ByteArrayToBmp(byte[] source)
        {
            using (var ms = new MemoryStream(source))
            {
                return new Bitmap(ms);
            }
        }
    }
}