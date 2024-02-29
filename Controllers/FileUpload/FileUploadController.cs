using IEMS_FrontApplications.Interface.FileUpload;
using IEMS_FrontApplications.Models.FileUpload;
using IEMS_WEB.Comman;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IEMS_FrontApplications.Controllers.FileUpload
{
    public class FileUploadController : Controller
    {
        private readonly IFileUpload _fileupload;
        public FileUploadController(IFileUpload fileUpload)
        {
            _fileupload = fileUpload;
        }
        [HttpGet]
        public async Task<IActionResult> GetFiles()
        {
            TempData.Remove("FileData");
           
            string FormCode = Convert.ToString(TempData.Peek("FormCode"));
            int TransactionId = Convert.ToInt32(TempData.Peek("FileRefId"));

            List<FileUploadResponse> lstFiles = await _fileupload.GetFiles(FormCode, TransactionId);
            return Json(new { data = lstFiles });
        }
        public async Task<IActionResult> StoreFileKeys(string filedata)
        {
            string res = string.Empty;
            try
            {
                var serializeData = JsonConvert.DeserializeObject<List<FileUploadRequestParameters>>(filedata);
                HttpContext.Session.SetComplexData("FileData", serializeData);////As per Rajveer Told Set Data to Session
                res = "1";
            }
            catch (Exception ex)
            {
                res = "-1";
            }
            return Json(res);
        }

        public async Task<IActionResult> DownloadFile(string filePath)
        {
            FileDownload response = await _fileupload.DownloadFile(filePath);
			return File(response.FileContent, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(filePath));
        }

    }
}