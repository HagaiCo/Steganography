using System;
using System.Text;

namespace WebApplication.Utilities
{
    public class MetaDataExe
    {
        public int GetIndexOfJunkkPE(byte[] file)
        {
            int j = 0;
            for (j = 0; j < file.Length; j++)
            {
                if ((file[j] == 46) && (file[j + 1] == 116))
                {
                    if ((file[j+2] == 101) && (file[j+3] == 120) && (file[j+4] == 116))
                    {
                        break;
                    }
                }
            }

            while (file[j] != 0)
            {
                j += 40;
            }
            return j+1;
        }
        
        public void HideMetaDataPE(byte[] file, byte [] encryptedMessage)
        {
            var PEMetaDataIndex = GetIndexOfJunkkPE(file);
            byte[] length = BitConverter.GetBytes(encryptedMessage.Length-32) ;
            file[PEMetaDataIndex++] = length[0];
            file[PEMetaDataIndex++] = length[1];
            file[PEMetaDataIndex++] = 0;
            file[PEMetaDataIndex++] = 0;
            foreach (var b in encryptedMessage)
            {
                file[PEMetaDataIndex++] = b;
            }
        }
        
        public byte[] SeekPE(byte[] file)
        {
            int j = 0;
            var PEMetaDataIndex = GetIndexOfJunkkPE(file);
            byte[] lengthBytes = {file[PEMetaDataIndex],file[PEMetaDataIndex+1],file[PEMetaDataIndex+2],file[PEMetaDataIndex+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] encryptedData=new byte[length];
            for (int i = PEMetaDataIndex + 4; i < PEMetaDataIndex + 4 + length; i++) //first 4 bytes for length
            {
                encryptedData[j++] = file[i];
            }
            return encryptedData;
            
        }
        
        public  byte[] ExtractKeyPE(byte [] file)
        {
            byte [] key = new byte[16];
            int j = 0;
            var PEMetaDataIndex = GetIndexOfJunkkPE(file);
            byte[] lengthBytes = {file[PEMetaDataIndex],file[PEMetaDataIndex+1],file[PEMetaDataIndex+2],file[PEMetaDataIndex+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            int index = PEMetaDataIndex + 4 + length;
            for (int i = index; i < index + 16; i++)
            {
                key[j++] = file[i];
            }

            return key;
        }
        
        public  byte[] ExtractIvPE(byte [] file)
        {
            byte [] iv = new byte[16];
            int j = 0;
            var PEMetaDataIndex = GetIndexOfJunkkPE(file);
            byte[] lengthBytes = {file[PEMetaDataIndex],file[PEMetaDataIndex+1],file[PEMetaDataIndex+2],file[PEMetaDataIndex+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            int index = PEMetaDataIndex + 4 + length + 16;
            for (int i = index; i < index + 16; i++)
            {
                iv[j++] = file[i];
            }
            return iv;
        }
    }
}