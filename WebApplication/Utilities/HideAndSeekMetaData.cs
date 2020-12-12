﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace WebApplication.Utilities
{
    public class HideAndSeekMetaData
    {

        public void hide(byte[] video, byte [] encryptedData)
        {
            
            var junkStart = findJUNK(video);
            var junkEnd = findLIST(video,junkStart);
            byte[] length = BitConverter.GetBytes(encryptedData.Length-32) ;
            video[junkStart++] = length[0];
            video[junkStart++] = length[1];
            video[junkStart++] = 0;
            video[junkStart++] = 0;
            foreach (var b in encryptedData)
            {
                video[junkStart++] = b;
            }
        }

        public byte[] Seek(byte[] video)
        {
            int j = 0;
            var junkStart = findJUNK(video);
            var junkEnd = findLIST(video,junkStart);
            byte[] lengthBytes = {video[junkStart],video[junkStart+1],video[junkStart+2],video[junkStart+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] encryptedData=new byte[length];
            
            for (int i = junkStart + 4; i < junkStart + 4 + length; i++) //first 4 bytes for length
            {
                encryptedData[j++] = video[i];
            }
            return encryptedData;
        }

        public byte[] ExtractKey(byte[] video)
        {
            byte [] key = new byte[16];
            int j = 0;
            var junkStart = findJUNK(video);
            byte[] lengthBytes = {video[junkStart],video[junkStart+1],video[junkStart+2],video[junkStart+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            int index = junkStart + 4 + length;
            for (int i = index; i < index + 16; i++)
            {
                key[j++] = video[i];
            }

            return key;
        }
        
        public byte[] ExtractIv(byte[] video)
        {
            byte [] iv = new byte[16];
            int j = 0;
            var junkStart = findJUNK(video);
            byte[] lengthBytes = {video[junkStart],video[junkStart+1],video[junkStart+2],video[junkStart+3]};
            int length = BitConverter.ToInt32(lengthBytes, 0);
            int index = junkStart + 4 + length + 16;
            for (int i = index; i < index + 16; i++)
            {
                iv[j++] = video[i];
            }

            return iv;
        }
        
        
        
        public static int findJUNK(byte[] b)
        {
            for (int i = 12; i < b.Length; i++)
            {
                if(b[i]!=74)
                    continue;
                else
                {
                    if (b[i + 1] == 85 && b[i + 2] == 78 && b[i + 3] == 75)
                        return i+20;
                }
            }

            return 0;
        }

        public static int findLIST(byte[] b, int junkStart)
        {
            for (int i = junkStart; i < b.Length; i++)
            {
                if(b[i]!=76)
                    continue;
                else
                {
                    if (b[i + 1] == 73 && b[i + 2] == 83 && b[i + 3] == 84)
                        return i-8;
                }
            }

            return 0;
        }

    }
}