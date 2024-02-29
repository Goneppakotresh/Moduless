using IEMS_FrontApplications.Comman;
using IEMS_WEB.Areas.MasterModule.Interface.Master;
using IEMS_WEB.Areas.RetailOn.Interface;
using IEMS_WEB.Areas.RetailOn.Models;
using IEMS_WEB.Areas.SpiritImport.Interface;
using IEMS_WEB.Areas.SpiritImport.Models.Response;
using IEMS_WEB.Comman;
using IEMS_WEB.Interface;
using IEMS_WEB.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IEMS_WEB.Controllers
{
    [SessionExpirationFilter]
    public class CreateUserController : Controller
    {
        SessionDetails objSession;
        private void SetSessionValue()
        {
            objSession = HttpContext.Session.GetComplexData<SessionDetails>("SessionDetails");
        }
        private readonly IOTP _iOTP;
        private readonly ICreateUser _iCreateUser;
        private readonly IDropDownList _iDropDownList;
        public CreateUserController(ICreateUser iCreateUser, IDropDownList iDropDownList, IOTP iOTP)
        {
            _iCreateUser = iCreateUser;
            _iDropDownList = iDropDownList;
            _iOTP = iOTP;
        }
        public async Task<ActionResult> CreateUsers(string EditViewKey="",string UserId="")
        {
            SetSessionValue();
            CreateUserResponseModel ObjRes = new CreateUserResponseModel();
            DropDownModel ObjDd = new DropDownModel();
  
            ObjDd.DropDownType = "LOCATIONTYPE";
            ObjRes.model.lstLocationType = await _iDropDownList.GetDropDown(ObjDd);
            ObjRes.model.lstUserType = await _iCreateUser.GetCreateUserDropDown("USERTYPE");
            if(UserId == "")
            {
                ObjRes.model.lstRole = await _iCreateUser.GetCreateUserDropDown("USER_ROLE", 0);
            }
            else
            {
                ObjDd.DropDownType = "ROLE";
                ObjRes.model.lstRole = await _iDropDownList.GetDropDown(ObjDd);
            }

            if (EditViewKey!="" && UserId != "")
            {
                ObjRes.model.EditViewKey = EditViewKey;
                ObjRes.model.UserId = Convert.ToInt32(UserId);
            }
            return View(ObjRes);
        }
        [HttpPost]
        public async Task<ActionResult> CreateUsers(CreateUserResponseModel obj)
        {
            SetSessionValue();
            obj.model.CreatedBy = objSession.UserID;
            CreateUserResponseModel ObjRes = new CreateUserResponseModel();
            ObjRes.ResponseModel = await _iCreateUser.CreateUserDetails(obj);
            return View(ObjRes);
        }

        public async Task<ActionResult> CreateUsersView()
        {
            CreateUserResponseModel ObjRes = new CreateUserResponseModel();
          
            return View(ObjRes);
        }
        public async Task<ActionResult> GetUserDetails()
        {
            SetSessionValue();
            CreateUserResponseModel objRes = new CreateUserResponseModel();
            objRes = await _iCreateUser.FetchUserDetails();
            return Json(new { data = objRes.LstUser });
        }

        public async Task<ActionResult> CheckIfDataUsed(string Value, int UserId, string Code)
        {
            SetSessionValue();
            CreateUserResponseModel objRes = new CreateUserResponseModel();
            bool DataExits = await _iCreateUser.CheckIfDataUsed(Value, UserId, Code);
            return Json(new { data = DataExits });
        }

        public async Task<JsonResult> ChangeLocationDropDown(int LocationTypeId)
        {
            SetSessionValue();
            DropDownModel ObjDd = new DropDownModel();
            ObjDd.DropDownType = "FILTERLOC";
            ObjDd.ParentID = LocationTypeId;
            List<SelectListItem> lstLocation = await _iDropDownList.GetDropDown(ObjDd);
            return Json(lstLocation);
        }
        public async Task<JsonResult> GetCreateUserDropDown(string UserTypeCode)
        {
            SetSessionValue();
            List<SelectListItem> lstDropDown = await _iCreateUser.GetCreateUserDropDown(UserTypeCode);
            return Json(lstDropDown);
        }
        public async Task<ActionResult> CheckFieldsRequired(int UserType)
        {
            SetSessionValue();
            FieldsRequired ObjDd = new FieldsRequired();
            ObjDd = await _iCreateUser.CheckFieldsRequired(UserType);
            return Json(new { data = ObjDd });
        }

        public async Task<ActionResult> GenerateOtp(string MobileNo)
        {
            string ResponseMessage = "";
            try
            {
                CreateUserResponseModel objRes = new CreateUserResponseModel();

                ResponseMessage = await _iOTP.GenerateTotp(MobileNo);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return Json(ResponseMessage);
        }
        public ActionResult ViewOTPScreen(string MobileNo)
        {
            OTP model = new OTP();
            try
            {
                model.MobileNumber = MobileNo;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return PartialView("_ValidateOTP", model);
        }


        public async Task<ActionResult> EditOrViewUserDetails(int UserId)
        {
            SetSessionValue();
            CreateUserResponseModel ObjRes = new CreateUserResponseModel();
            ObjRes = await _iCreateUser.EditOrViewUserDetails(UserId);
            DropDownModel ObjDd = new DropDownModel();
            ObjDd.DropDownType = "FILTERLOC";
            ObjDd.ParentID = ObjRes.model.LocationType;
            ObjRes.model.lstLocation = await _iDropDownList.GetDropDown(ObjDd);
            FieldsRequired ObjFR = new FieldsRequired();
            ObjFR = await _iCreateUser.CheckFieldsRequired(ObjRes.model.UserType);
            ObjRes.model.lstOtherDept = await _iCreateUser.GetCreateUserDropDown(ObjFR.UserTypeCode);
           // ObjRes.model.lstRole = await _iCreateUser.GetCreateUserDropDown("USER_ROLE", ObjRes.model.UserType);


            return Json(new { data = ObjRes });
        }
        public async Task<IActionResult> UpdateActiveStatus(string UserId)
        {
            UpdateUserStatusResponseModel obj = new UpdateUserStatusResponseModel();
            obj = await _iCreateUser.UpdateActiveStatus(Convert.ToInt32(UserId)); 
            return Json(obj);
        }
        public async Task<ActionResult> GetUserRole(string Key, string Type)
        {
            CreateUserResponseModel ObjRes = new CreateUserResponseModel();
           
            ObjRes.model.lstUserType = await _iCreateUser.GetCreateUserDropDown(Type,Convert.ToInt32(Key));
            return Json(new { data = ObjRes.model.lstUserType });
        }

    }
}