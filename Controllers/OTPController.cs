using IEMS_WEB.Interface;
using IEMS_WEB.Models;
using Microsoft.AspNetCore.Mvc;

namespace IEMS_WEB.Controllers
{
    public class OTPController : Controller
    {
        // GET: OTP
        private readonly IOTP _iOTP;
        public OTPController(IOTP iOTP)
        {
            _iOTP = iOTP;
        }
       
        [HttpGet]
        public async Task<ActionResult> ViewOTPPartialView(string MobileNumber, string FormKey)
        {
            OTPViewModel OTPPartialModel = new OTPViewModel();
            OTPPartialModel.OTPObj.MobileNumber = MobileNumber;
            OTPPartialModel.OTPObj.OTPKey = FormKey;
            string ResponseMessage = await _iOTP.GenerateTotp(Convert.ToString(MobileNumber));
            OTPPartialModel.ResponseMessage = ResponseMessage;
            return PartialView("_OTP", OTPPartialModel);
        }   
        [HttpPost]
        public async Task<ActionResult> ValidateOTP(OTP OTPvalidateReq)
        {
            OTPResponse model = new OTPResponse();
            model = await _iOTP.ValidateTotp(OTPvalidateReq);
            return Json(model);
        }
    }
}