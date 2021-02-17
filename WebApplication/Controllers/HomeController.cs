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
using WebApplication.Utilities;

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
                if (fileDataUploadRequestModel.FileAsHttpPostedFileBase?.ContentLength > 0)
                {
                    var path = Path.Combine(Server.MapPath("~/Content/images/"), 
                        fileDataUploadRequestModel.FileAsHttpPostedFileBase.FileName);
                    fileDataUploadRequestModel.FilePath = path;
                    fileDataUploadRequestModel.FileAsHttpPostedFileBase.SaveAs(path);
                    
                    uploadSucceed = await _homeService.Upload(fileDataUploadRequestModel);
                    

                    if (uploadSucceed)
                        ModelState.AddModelError(string.Empty, "Uploaded Successfully");
                    
                }
                else
                {
                    throw new Exception("File are empty");
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

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetAllAssignedFileData()
        {
            var allPermittedFilesData = await _homeService.GetPermittedFilesData();
            if (allPermittedFilesData == null) return View();
            
            var filesToPresent = new List<FileDataDownloadResponseModel>();
            foreach (var file in allPermittedFilesData)
            {
                var tt = file.Convert();
                var obj = new FileDataDownloadResponseModel();
                obj.File = file.File;
                obj.FileName = file.FileName;
                obj.Id = file.Id;
                obj.FileExtension = file.FileExtension;
                obj.FileType = file.FileType;
                obj.EncryptionMethod = file.EncryptionMethod;
                obj.HidingMethod = file.HidingMethod;
                obj.SecretMessage = _homeService.ExtractMessage(tt);
                
                filesToPresent.Add(obj);
            }
            return View(filesToPresent);
        }

        public ActionResult ShowSecretMessage(FileDataUploadRequestModel fileData)
        {
            return Content(fileData.SecretMessage);
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