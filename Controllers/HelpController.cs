using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using IEMS_FrontApplications.Models.FileUpload;
using IEMS_WEB.Areas.OrderForSupply.Interface;
using IEMS_WEB.Comman;
using IEMS_WEB.Interface;
using IEMS_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace IEMS_WEB.Controllers
{
    public class HelpController : Controller
    {
        SessionDetails objSession;
        private void SetSessionValue()
        {
            objSession = HttpContext.Session.GetComplexData<SessionDetails>("SessionDetails");
        }
        private readonly IHelp _HelpRepo;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _IHttpContextAccessor;

        public HelpController(IHelp HelpRepo, IDropDownList IDropDownList, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, IHttpContextAccessor ihttpContextAccessor)
        {
            _HelpRepo = HelpRepo;
            _hostingEnvironment = hostingEnvironment;
            _IHttpContextAccessor = ihttpContextAccessor;
        }

        public async Task<ActionResult> HelpView()
        {
            SetSessionValue();
            HelpViewList obj = new HelpViewList();
            obj.RoleName = _IHttpContextAccessor.HttpContext.Session.GetString("RoleId");

            return View(obj);
        }
        [HttpGet]
        public async Task<ActionResult> Help()
        {
            SetSessionValue();
            HelpModelRequest obj = new HelpModelRequest();
            DropDownModel ObjDropDown = new DropDownModel();
            obj.RoleName = _IHttpContextAccessor.HttpContext.Session.GetString("RoleId");
            obj.listDocumentType = await _HelpRepo.GetHelpFormDropDown("DOCUMENT_TYPE");

            return View(obj);
        }

        public async Task<ActionResult> GetHelpDetails()
        {
            SetSessionValue();
            HelpViewList objlist = await _HelpRepo.GetHelpDetails();
            return Json(new { data = objlist.lstHelpView });
        }

        [HttpPost]
        public async Task<ActionResult> Help(HelpModelRequest objRequest, IFormFile file)
        {
            SetSessionValue();
            string UserId = _IHttpContextAccessor.HttpContext.Session.GetString("UserId");
            objRequest.CreatedBy = Convert.ToInt64(UserId);
            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
            {
                // Read the contents of the IFormFile directly into a byte array
                byte[] fileBytes = binaryReader.ReadBytes((int)file.Length);
                // Convert the byte array to a Base64 string
                string base64String = Convert.ToBase64String(fileBytes);
                // Now 'base64String' contains the Base64 representation of the file
                objRequest.base64file = base64String;
            }
            objRequest.FileName= file.FileName;
            HelpModelResponse objResponse = await _HelpRepo.PostHelpDetails(objRequest);
            objRequest.Status= objResponse.Status;
            objRequest.Message = objResponse.Message;

            return View(objRequest);
        }
        public async Task<IActionResult> DownloadFile(string filePath)
        {
            HelpFileDownload response = await _HelpRepo.DownloadFile(filePath);
            return File(response.FileContent, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(filePath));
        }
    }
}
