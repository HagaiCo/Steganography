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
                TextToHide = data.TextToHide
            };
            return obj;
        }
        
        public static Image byteArrayToImage(byte[] bytesArr)
        {
            using MemoryStream memstr = new MemoryStream(bytesArr);
            var img = Image.FromStream(memstr);
            return img;
        }
    }
}