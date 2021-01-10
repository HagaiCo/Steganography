using System.Collections.Generic;

namespace WebApplication.ResponseModel
{
    public class FileDataUploadResponseModel
    {
        public byte [] File { get; set; }
        
        public string FileName { get; set; }

        public FileType FileType { get; set; }
        
        public EncryptionMethod EncryptionMethod { get; set; }
        
        public HidingMethod HidingMethod { get; set; }
        
        public string FileExtension { get; set; }
        
        public string Id { get; set; }

        public string SecretMessage { get; set; }
        
        public List<string> PermittedUsers { get; set; }
        
        
    }

    public enum FileType
    {
        UndefinedType = 0,
        Image = 1,
        Audio =2,
        Video = 3,
        Executable = 4,
        UnknownType = -1
    }

    public enum EncryptionMethod
    {
        Aes =0,
        Serpent = 1
    }

    public enum HidingMethod
    {
        Lsb=0,
        MetaData=1
    }
}