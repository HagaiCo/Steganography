using System.Collections.Generic;

namespace WebApplication.ResponseModel
{
    public class FileDataUploadResponseModel
    {
        public byte [] File { get; set; }
        
        public string FileName { get; set; }

        public string Id { get; set; }

        public string TextToHide { get; set; }
        
        public List<string> PermittedUsers { get; set; }
    }
}