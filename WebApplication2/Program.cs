using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Initializer();
            CreateUser().GetAwaiter();
            //CreateHostBuilder(args).Build().Run();
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest) WebRequest.Create("ftp://www.contoso.com/test.htm");
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("anonymous", "janeDoe@contoso.com");

            // Copy the contents of the file to the request stream.
            byte[] fileContents;
            using (StreamReader sourceStream = new StreamReader("testfile.txt"))
            {
                fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            }

            request.ContentLength = fileContents.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            using (FtpWebResponse response = (FtpWebResponse) request.GetResponse())
            {
                Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        public static async Task<bool> CreateUser()
        {
            UserRecordArgs args = new UserRecordArgs()
            {
                Email = "user@example.com",
                EmailVerified = false,
                PhoneNumber = "+11234567890",
                Password = "secretPassword",
                DisplayName = "John Doe",
                PhotoUrl = "http://www.example.com/12345678/photo.png",
                Disabled = false,
            };
            UserRecord userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);
            // See the UserRecord reference doc for the contents of userRecord.
            Console.WriteLine($"Successfully created new user: {userRecord.Uid}");
            return true;
        }

        private static IFirebaseClient client;
        public static void Initializer()
        {
            
            string path = AppDomain.CurrentDomain.BaseDirectory + @"steganography-5582f-firebase-adminsdk-f3l4k-47ce03a2fa.json"; 
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            FirestoreDb db = FirestoreDb.Create("steganography-5582f");
            
            /*//InitializeComponent();
            IFirebaseConfig config = new FirebaseConfig()
            {
                AuthSecret = "jnUAPjDAutWRVPuwEO4QcRJIj1bsmZc8plnSrM9K",
                BasePath = "https://steganography-5582f.firebaseio.com/"
            };
            
            client = new FirebaseClient(config);
            
            /*FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.GetApplicationDefault(),
            });#1#
            if (client != null)
            {
                Console.WriteLine("Success!!!");            
            }*/
        }
    }
}