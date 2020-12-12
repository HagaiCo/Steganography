using System.Drawing;

namespace WebApplication.ResponseModel
{
    public class FileDataDownloadResponseModel
    {
        public byte[] File { get; set; }
        
        public string FileName { get; set; }

        public string Id { get; set; }
        
        public FileType FileType { get; set; }
        
        public EncryptionMethod EncryptionMethod { get; set; }
        
        public HidingMethod HidingMethod { get; set; }
       

    }
    
    
}