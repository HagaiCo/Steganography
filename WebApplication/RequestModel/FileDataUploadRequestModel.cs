using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using WebApplication.ResponseModel;

namespace WebApplication.RequestModel
{
    public class FileDataUploadRequestModel
    {
        [Required]
        [Display(Name = "Choose file to upload")] 
        public HttpPostedFileBase File { get; set; }
        
        [Key] 
        public string Id { get; set; }
        [Required]
        [Display(Name = "Type text to hide")] 
        public string SecretMessage { get; set; }
        [Required] 
        [Display(Name = "Choose permitted users")] 
        public List<string> PermittedUsers { get; set; }
        public string FilePath { get; set; }
        
        public FileType FileType { get; set; }
        
        
        
        public EncryptionMethod EncryptionMethod { get; set; }
        [Display(Name="Choose hiding method")]
        public HidingMethod HidingMethod { get; set; }
        
     
    }
    
    
}