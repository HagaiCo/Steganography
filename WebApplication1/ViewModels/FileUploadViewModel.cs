using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Models
{
    public class FileUploadViewModel
    {
        public HttpPostedFileBase File { get; set; }
        
        [Key] 
        public string Id { get; set; }
        [Required]
        [Display(Name = "Text To Hide")] 
        public string TextToHide { get; set; }
        [Required] 
        public List<string> PermittedUsers { get; set; }
        public string FilePath { get; set; }

    }
}