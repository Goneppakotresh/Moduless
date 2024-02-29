using IEMS_FrontApplications.Interface.FileUpload;
using IEMS_FrontApplications.Models.FileUpload;
using IEMS_WEB.Comman;
using IEMS_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace IEMS_WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileUpload _IFileUpload;
        public HomeController(ILogger<HomeController> logger, IFileUpload IFileUpload)
        {
            _logger = logger;
            _IFileUpload = IFileUpload;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult TESTFILE()
        {
            TempData.Remove("FormCode");
            TempData.Remove("FileRefId");
            TempData.Remove("FileData");

            TempData["FormCode"] = "TESTPAGE";
            TempData.Keep("FormCode");
            TempData["FileRefId"] = 0;
            TempData.Keep("FileRefId");
            return View();
        }
        [HttpPost]
        public IActionResult TESTFILE(string SS)
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            int Rndno = _rdm.Next(_min, _max);
            IFormFileCollection files = Request.Form.Files;
            UploadFile(files, "TESTPAGE", Rndno);

            TempData.Remove("FormCode");
            TempData.Remove("FileRefId");
            TempData.Remove("FileData");

            TempData["FormCode"] = "TESTPAGE";
            TempData.Keep("FormCode");

            TempData["FileRefId"] = Rndno;


            TempData.Keep("FileRefId");
            return View();
        }
        private void UploadFile(IFormFileCollection files, string formCode, int status)
        {
            FileUploadRequest request = new FileUploadRequest();
            request.FormCode = formCode;
            request.TransactionId = status;

            var filedata = HttpContext.Session.GetComplexData<List<FileUploadRequestParameters>>("FileData");
            if (filedata != null)
            {
                int i = 0;

                foreach (IFormFile source in files)
                {
                    if (i < filedata.Count)
                    {
                        //using (var target = new MemoryStream())
                        //{
                        //    source.CopyToAsync(target);
                        //    target.ToArray();
                        //    source.CopyToAsync(target);
                        //    var filecontent = target.ToArray();
                        //    request.lstFiles.Add(new FileUploadRequestParameters { FileName = source.FileName, ValueField = filedata.Where(y => y.FileName == source.FileName).Select(x => x.ValueField).FirstOrDefault(), FileId = Convert.ToInt16(filedata.Where(y => y.FileName == source.FileName).Select(x => x.FileId).FirstOrDefault()), base64file = Convert.ToBase64String(filecontent) });
                        //}
                        using (var binaryReader = new BinaryReader(source.OpenReadStream()))
                        {
                            // Read the contents of the IFormFile directly into a byte array
                            byte[] fileBytes = binaryReader.ReadBytes((int)source.Length);
                            // Convert the byte array to a Base64 string
                            string base64String = Convert.ToBase64String(fileBytes);
                            // Now 'base64String' contains the Base64 representation of the file
                            request.lstFiles.Add(new FileUploadRequestParameters { FileName = source.FileName, ValueField = filedata.Where(y => y.FileName == source.FileName).Select(x => x.ValueField).FirstOrDefault(), FileId = Convert.ToInt16(filedata.Where(y => y.FileName == source.FileName).Select(x => x.FileId).FirstOrDefault()), base64file = base64String });


                        }
                        i = i + 1;
                    }
                }

                _IFileUpload.UploadFiles(request);
            }
            HttpContext.Session.Remove("FileData");
        }
    }
}