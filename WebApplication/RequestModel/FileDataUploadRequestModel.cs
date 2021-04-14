using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using WebApplication.ResponseModel;

namespace WebApplication.RequestModel
{
    public class FileDataUploadRequestModel
    {
        // [Required(ErrorMessage = "File is required.")]
        // [Display(Name = "Choose file to upload")] 
        public HttpPostedFileBase File { get; set; }
        [Key]
        public string Id { get; set; }
        [Required(ErrorMessage = "File is required.")]
        [Display(Name = "Choose file to upload")]
        public HttpPostedFileBase FileAsHttpPostedFileBase { get; set; }
        [Required]

        public byte[] FileAsByteArray { get; set; }
        public string FileName { get; set; }
        [Required(ErrorMessage = "Text to hide is required.")]
        [Display(Name = " ")] 
        public string SecretMessage { get; set; }
        [Required(ErrorMessage = "Permitted user is required.")]
        [Display(Name = "Choose permitted users")] 
        public List<string> PermittedUsers { get; set; }

        public string SharingUser { get; set; }
        [Required]
        public string FilePath { get; set; }
        public FileType FileType { get; set; }
        [Required(ErrorMessage = "Encryption Method is required.")]
        [Display(Name="Choose Encryption Method")]
        public EncryptionMethod EncryptionMethod { get; set; }
        [Required(ErrorMessage = "Hiding Method is required.")]
        [Display(Name="Choose Hiding Method")]
        public HidingMethod HidingMethod { get; set; }
        [Required]
        public string FileExtension { get; set; }
        
        
    }
}