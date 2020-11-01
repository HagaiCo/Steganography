using WebApplication.Models;
using WebApplication.ResposeModel;

namespace WebApplication.Utilities
{
    public static class TransformationUtils
    {
        public static FileDataUploadResponseModel Convert(this FileDataUploadRequestModel data)
        {
            var obj = new FileDataUploadResponseModel
            {
                File = System.IO.File.ReadAllBytes(data.FilePath),
                Id = data.Id,
                FileName = data.File.FileName,
                PermittedUsers = data.PermittedUsers,
                TextToHide = data.TextToHide
            };
            return obj;
        }
    }
}