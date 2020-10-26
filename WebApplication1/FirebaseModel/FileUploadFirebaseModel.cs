using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WebApplication1.FirebaseModel
{
    public class FileUploadFirebaseModel
    {
        [Key] 
        public string Id { get; set; }
        public byte[] File { get; set; }
        public string FileName { get; set; }
        [Required] 
        public List<string> PermittedUsers { get; set; }
    }
}