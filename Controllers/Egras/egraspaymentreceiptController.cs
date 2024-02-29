using IEMS_FrontApplications.Comman;
using IEMS_WEB.Interface.Egras;
using IEMS_WEB.Models.Egras;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IEMS_WEB.Controllers.Egras
{
    public class egraspaymentreceiptController : Controller
    {
        private readonly IEgras _EgrasRepo;


        public egraspaymentreceiptController(IEgras EgrasRepo)
        {
            _EgrasRepo = EgrasRepo;

        }
        public ActionResult TEST()
        {
            return View();
        }
        [Route("egraspaymentreceipt")]
        [HttpPost]
        public async Task<ActionResult> Index()
        {
            
            string GRN = "";
            string Message = "Challan Failed";
            EGrasPaymentRequest PaymentRequest = new EGrasPaymentRequest();
            EGrasPaymentRespose PaymentRespose = new EGrasPaymentRespose();
            var headerexist = Request.Form.TryGetValue("encdata", out var headerValues);            
            if (headerexist)
            {
                //Get Egras Request
                PaymentRequest.Encdata = headerValues;
                PaymentRespose = await _EgrasRepo.EGrasPayment(PaymentRequest);

                if (PaymentRespose != null)
                {

                    GRN = PaymentRespose.GRN;
                    Message = PaymentRespose.Message;

                }
            }
            ViewBag.GRN = GRN;
            ViewBag.Message = Message;
            ViewBag.headerVal = headerValues;

            return View();
        }


        //[Route("egraspaymentreceipt")]
        //[HttpPost]
        //public async Task<ActionResult> Index()
        //{
        //    //Call Api
        //    string GRN = "";
        //    string Message = "Challan Failed";
        //    EGrasPaymentRequest PaymentRequest = new EGrasPaymentRequest();

        //    EGrasPaymentRespose PaymentRespose = new EGrasPaymentRespose();
        //    if (Request.Headers["encdata"] != "")
        //    {
        //        PaymentRequest.Encdata = "3BIq1IujkSL2Y5+WlyBfD20SpcTy2ydVcPskpe9XIyNB6pejQ6p743bhhrKOA763k0vzFyI04byospS4wH1U6N6JfO8jwaNzm1PzS8zd2iLY0LDXiHuYcs9kohvntUuNlji//SlEnuOWB2Fmzkj17bCFIO9yTgw8EbLKWraf8XdSmMazY28psre5EpAfQGBtX9lwGxJZbIzWEIQ1dCMBktEq3Jctu+Jq3K9+Zi7z73bmlxmWjnkTANhSkHWBrU6T";

        //        PaymentRespose = await _EgrasRepo.EGrasPayment(PaymentRequest);

        //        if (PaymentRespose != null)
        //        {

        //            GRN = PaymentRespose.GRN;
        //             Message = PaymentRespose.Message;

        //        }
        //    }
        //    ViewBag.GRN = GRN;
        //    ViewBag.Message = Message;


        //    return View();
        //}


    }
}