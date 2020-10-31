using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WebApplication.FirebaseModel
{
    public class FileUploadFirebaseRequest
    {
        [Key] 
        public string Id { get; set; }
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string TextToHide { get; set; }
        [Required] 
        public List<string> PermittedUsers { get; set; }
    }
}