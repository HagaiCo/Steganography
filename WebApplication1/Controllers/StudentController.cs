﻿using System;
using System.EnterpriseServices.Internal;
using System.Web.Mvc;
using FireSharp;
using FireSharp.Response;
using FireSharp.Interfaces;
using FireSharp.Config;
using System.Web.Mvc;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    //[Authorize]
    public class StudentController : Controller
    {
        static IFirebaseConfig config = new FirebaseConfig()
        {
            AuthSecret = "jnUAPjDAutWRVPuwEO4QcRJIj1bsmZc8plnSrM9K",
            BasePath = "https://steganography-5582f.firebaseio.com/"
        };
        FirebaseClient client = new FirebaseClient(config);
        
        
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        

        [HttpPost]
        public ActionResult Create(Student student)
        {
            try
            {
                AddStudentToFirebase(student);
                ModelState.AddModelError(string.Empty, "Added Successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ModelState.AddModelError(string.Empty, e.Message);
                throw;
            }
            return View();
        }

        private void AddStudentToFirebase(Student student)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = student;
            PushResponse response = client.Push("Student/", data);
            data.StudentId = response.Result.name;
            SetResponse setResponse = client.Set("Student/" + data.StudentId, data);
        }
    }
}