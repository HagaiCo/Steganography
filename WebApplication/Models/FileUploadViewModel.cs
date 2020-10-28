using Microsoft.AspNetCore.Http;

namespace WebApplication1.Models
{
    public class FileUploadViewModel
    {
        public IFormFile File { get; set; }

    }
}