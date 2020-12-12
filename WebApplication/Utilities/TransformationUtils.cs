using System.Drawing;
using System.IO;
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
                FileName = data.File.FileName,
                PermittedUsers = data.PermittedUsers,
                SecretMessage = data.SecretMessage
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