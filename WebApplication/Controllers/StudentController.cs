using System;
using System.EnterpriseServices.Internal;
using System.Web.Mvc;
using FireSharp;
using FireSharp.Response;
using FireSharp.Interfaces;
using FireSharp.Config;
using System.Web.Mvc;
using WebApplication.Models;


namespace WebApplication.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        static IFirebaseConfig config = new FirebaseConfig()
        {
            AuthSecret = "jnUAPjDAutWRVPuwEO4QcRJIj1bsmZc8plnSrM9K",
            BasePath = "https://steganography-5582f.firebaseio.com/"
        };
        FirebaseClient _client = new FirebaseClient(config);

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        

        [HttpPost]
        public ActionResult Create(StudentRequestModel studentRequestModel)
        {
            try
            {
                AddStudentToFirebase(studentRequestModel);
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

        private void AddStudentToFirebase(StudentRequestModel studentRequestModel)
        {
            _client = new FirebaseClient(config);
            var data = studentRequestModel;
            PushResponse response = _client.Push("Student/", data);
            data.StudentId = response.Result.name;
            SetResponse setResponse = _client.Set("Student/" + data.StudentId, data);
        }
    }
}