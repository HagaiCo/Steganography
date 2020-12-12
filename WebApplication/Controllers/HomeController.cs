using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services;
using WebApplication.RequestModel;
using WebApplication.ResponseModel;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly HomeService _homeService = new HomeService();
        private static readonly AccountService _accountService = new AccountService();

        public ActionResult UploadFileData()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public async Task<ViewResult> UploadFileData(FileDataUploadRequestModel fileDataUploadRequestModel)
        {
            try
            {
                FileStream stream;
                bool uploadSucceed;
                if (fileDataUploadRequestModel.File.ContentLength > 0)
                {
                    var path = Path.Combine(Server.MapPath("~/Content/images/"),
                        fileDataUploadRequestModel.File.FileName);
                    fileDataUploadRequestModel.FilePath = path;
                    fileDataUploadRequestModel.File.SaveAs(path);
                    
                    uploadSucceed = await _homeService.Upload(fileDataUploadRequestModel);
                    

                    if (uploadSucceed)
                        ModelState.AddModelError(string.Empty, "Uploaded Successfully");
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception was thrown: {ex}");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
            return View();
        }

        //[HttpPost]
        [Authorize]
        public async Task<ActionResult> GetAllAssignedFileData()
        {
            var allPermittedFilesData = _homeService.GetPermittedFilesData();
            if (allPermittedFilesData == null) return View();
            var filesToPresent = new List<FileDataDownloadResponseModel>();
            foreach (var file in allPermittedFilesData)
            {
                var obj = new FileDataDownloadResponseModel();
                obj.File = file.File;
                obj.FileName = file.FileName;
                obj.Id = file.Id;
                filesToPresent.Add(obj);
            }
            return View(filesToPresent);
        }

        public ActionResult ShowSecretMessage(string fileId)
        {
            var message = _homeService.ExtractMessage(fileId);
            //var message = _homeService.GetSecretMessageFromVideo(fileId);
            
            return Content(message);
        }
        public ActionResult DownloadFileData(string fileId, string fileName)
        {
            try
            {
                _homeService.DownloadFile(fileId);
                ModelState.AddModelError(string.Empty, $"The file {fileName} was download successfully");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return RedirectToAction("GetAllAssignedFileData", "Home");
        }

        public ActionResult DeleteFileData(string fileId, string fileName)
        {
            try
            {
                _homeService.DeleteFileData(fileId);
                ModelState.AddModelError(string.Empty, $"The file {fileName} was deleted successfully");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return RedirectToAction("GetAllAssignedFileData", "Home");        }
    }
}