using System.Web.Mvc;
using FireSharp.Interfaces;
using System;
using System.Configuration;
using System.Collections.Specialized;
using FireSharp;

namespace WebApplication.Services
{
    public class BaseService
    {
        protected string ApiKey = ConfigurationManager.AppSettings["ApiKey"];
        public string Bucket = ConfigurationManager.AppSettings["Bucket"];
        protected string AdminEmail = ConfigurationManager.AppSettings["adminEmail"];
        protected string AdminPass = ConfigurationManager.AppSettings["adminPass"];
        protected string PasswordMonthTimeStamp = ConfigurationManager.AppSettings["passwordTimeStamp"];
        private static readonly string AuthSecret = ConfigurationManager.AppSettings["AuthSecret"];
        private static readonly string BasePath = ConfigurationManager.AppSettings["BasePath"];

        protected static IFirebaseConfig Config = new FireSharp.Config.FirebaseConfig()
        {
            AuthSecret = AuthSecret,
            BasePath = BasePath
        };
        protected FirebaseClient _client = new FirebaseClient(Config);
    }
}  