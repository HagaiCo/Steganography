using System.Collections.Generic;

namespace WebApplication.ResponseModel
{
    public class FileDataUploadResponseModel
    {
        public byte [] File { get; set; }
        
        public string FileName { get; set; }

        public FileType FileType { get; set; }
        
        public string Id { get; set; }

        public string TextToHide { get; set; }
        
        public List<string> PermittedUsers { get; set; }
    }

    public enum FileType
    {
        UndefinedType = 0,
        Image = 1,
        Video = 2,
        Executable = 3,
        UnknownType = -1
    }
}