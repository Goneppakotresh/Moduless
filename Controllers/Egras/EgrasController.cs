using IEMS_WEB.Interface.Egras;
using IEMS_WEB.Models.Egras;
using Microsoft.AspNetCore.Mvc;

namespace IEMS_WEB.Controllers.Egras
{
    public class EgrasController : Controller
    {

        private readonly IEgras _EgrasRepo;

        public EgrasController(IEgras EgrasRepo)
        {
            _EgrasRepo = EgrasRepo;
        }


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GETEGrasChallanDetail(EGrasChallanDetail EGrassChallanDetailModal)
        {
            EGrasChallanRequest Responsemodel = new EGrasChallanRequest();
            EGrasChallanResponse EGrasChallanResponse = new EGrasChallanResponse();

            Responsemodel = await _EgrasRepo.EGrasChallanDetail(EGrassChallanDetailModal);
            string json = "";

            if (Responsemodel != null)
            {


                if (Responsemodel.Status == 1)
                {
                    EGrasChallanResponse = await _EgrasRepo.EGrasChallanCreate(Responsemodel);

                    if (EGrasChallanResponse.Status == 1)
                    {
                        json = Newtonsoft.Json.JsonConvert.SerializeObject(EGrasChallanResponse);
                    }
                    else
                    {
                        EGrasChallanResponse.Status = 0;

                        json = Newtonsoft.Json.JsonConvert.SerializeObject(EGrasChallanResponse);
                    }
                }
                else
                {
                    EGrasChallanResponse.Status = 0;

                    json = Newtonsoft.Json.JsonConvert.SerializeObject(EGrasChallanResponse);
                }
            }
            else
            {
                EGrasChallanResponse.Status = 0;
                EGrasChallanResponse.Message = "Challan No Create";
                json = Newtonsoft.Json.JsonConvert.SerializeObject(EGrasChallanResponse);
            }
            return Json(json);
        }

        public ActionResult EgrassView(string Encdata, string Merchant_code, string AUIN)
        {
            ViewBag.Encdata = Encdata;
            ViewBag.Merchant_code = Merchant_code;
            ViewBag.AUIN = AUIN;
            return View();
        }

        public ActionResult EgrassData(string Encdata, string Merchant_code, string AUIN)
        {
            ViewBag.Encdata = Encdata;
            ViewBag.Merchant_code = Merchant_code;
            ViewBag.AUIN = AUIN;
            return Json("sucess");
        }

        public async Task<ActionResult> GetPaymentStatus(string formCode, decimal transactionId)
        {
            string[] res = await _EgrasRepo.GetPaymentStatus(formCode, transactionId);
            return Json(res);
        }

    }
}