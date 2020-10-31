using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WebApplication.ResposeModel
{
    public class FileDataUploadResponseModel
    {
        public byte[] File { get; set; }
        
        public string FileName { get; set; }

        public string Id { get; set; }

        public string TextToHide { get; set; }
        
        public List<string> PermittedUsers { get; set; }
    }
}