using IEMS_WEB.Comman;
using IEMS_WEB.Interface.WorkFlow;
using IEMS_WEB.Models;
using IEMS_WEB.Models.WorkFlow;
using Microsoft.AspNetCore.Mvc;

namespace IEMS_WEB.Controllers
{
    public class InboxController : Controller
    {
        SessionDetails objSession;
        private void SetSessionValue()
        {
            objSession = HttpContext.Session.GetComplexData<SessionDetails>("SessionDetails");
        }
        private readonly IAccessRights _IAccessRights;
        private readonly IWFInbox _IWFInbox;
        private readonly IWorkFlow _iWorkFlow;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InboxController(IAccessRights IAccessRights, IWFInbox iWFInbox, IWorkFlow iWorkFlow, IHttpContextAccessor ihttpContextAccessor)
        {
            _IAccessRights = IAccessRights;
            _IWFInbox = iWFInbox;
            _iWorkFlow = iWorkFlow;
            _httpContextAccessor = ihttpContextAccessor;
        }
        // GET: Inbox
        [HttpGet]
        public async Task<ActionResult> WFInbox()
        {
            WorkFlowInbox WorkFlowInbox = new WorkFlowInbox();
            AccessRights accessRights = new AccessRights();
            WorkFlowInbox.FromDate = DateTime.Now.AddDays(-30).ToString("dd-MMM-yyyy");
            WorkFlowInbox.ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
            WorkFlowDropDownModel workFlowDropDownModel = new WorkFlowDropDownModel();
            try
            {
                //WorkFlowInbox.getlistModule(WorkFlowInbox);
                //WorkFlowInbox.getlistBussinessobj(WorkFlowInbox);
                accessRights = await _IAccessRights.GetDropDown(accessRights);
                WorkFlowInbox.listmodules = accessRights.lstmodules;
                //accessRights = await _IAccessRights.FetchSubmodule(accessRights);
                //WorkFlowInbox.listbussinessobj = accessRights.lstsubmodules;
                workFlowDropDownModel.DropDownType = "SUB_MODULE";
                WorkFlowInbox.listbussinessobj = await _iWorkFlow.GetWFDropDown(workFlowDropDownModel);
                return View(WorkFlowInbox);
            }
            catch (Exception ex)
            {
                return View(WorkFlowInbox);
            }

        }
        //get datatable
        public async Task<ActionResult> Approvaldetails(string ActionType, int ModuleId, int bObjId, string FromDate, string ToDate)
        {
            SetSessionValue();
            string RoleId = _httpContextAccessor.HttpContext.Session.GetString("RoleId");
            string UserId = _httpContextAccessor.HttpContext.Session.GetString("UserId");
            string UserrefId = _httpContextAccessor.HttpContext.Session.GetString("UserRefId");
            string LocationCode = _httpContextAccessor.HttpContext.Session.GetString("LocationCode");

            WorkFlowInbox objin = new WorkFlowInbox();
            if (objSession != null)
            {
                objin.CreatedBy = objSession.UserID;
                //objin.LocCode = objSession.LicenseeCode;
                objin.CurrRoleId = Convert.ToInt32(objSession.RoleId);
                objin.LocCode = Convert.ToInt32(objSession.LocationCode);

            }

            objin.CurrRoleType = "R";
            objin.ActionType = Convert.ToInt32(ActionType);

            if (ActionType == "" || ActionType == null)
            {
                ActionType = "0";
            }
            if (ModuleId != 0)
            {
                objin.moduleid = ModuleId;
            }
            if (bObjId != 0)
            {
                objin.BObjId = bObjId;
            }
            if (FromDate != "")
            {
                objin.FromDate = FromDate;
            }
            if (ToDate != "")
            {
                objin.ToDate = ToDate;
            }
            objin = await _IWFInbox.loadApprovaldetails(objin);
            //if (objin.ActionType == 0)
            //    objin.ApprovalDetails = objin.ApprovalDetails.Where(x => x.statusId == 0).ToList();
            //else if (objin.ActionType == 1)
            //    objin.ApprovalDetails = objin.ApprovalDetails.Where(x => x.statusId == 1).ToList();
            //var jsonResult = new JsonResult();

            //jsonResult = Json(new { data = objin.ApprovalDetails });
            //jsonResult.MaxJsonLength = int.MaxValue;
            return Json(new { data = objin.ApprovalDetails });
        }

        public async Task<ActionResult> FetchApprovedHistory(String bObjId, String RecordId)
        {
            //SetSessionValue();
            string RoleId = _httpContextAccessor.HttpContext.Session.GetString("RoleId");
            string UserId = _httpContextAccessor.HttpContext.Session.GetString("UserId");
            string UserrefId = _httpContextAccessor.HttpContext.Session.GetString("UserRefId");

            WorkFlowInbox objApprovedHist = new WorkFlowInbox();

            if (bObjId == "0")
            {
                bObjId = "0";
            }
            if (RecordId == "0")
            {
                RecordId = "";
            }
            if (bObjId != "" && RecordId != "")
            {

                //objApprovedHist.LocCode = objSession.LocationCode;
                //if (objSession!=null)
                //{
                //    objApprovedHist.CreatedBy = objSession.UserID;
                //}
                objApprovedHist.CreatedBy = Convert.ToInt32(UserId);
                objApprovedHist.TransactionId = Convert.ToInt32(RecordId);
                objApprovedHist.BObjId = Convert.ToInt32(bObjId);
                objApprovedHist = await _IWFInbox.getApprovedHist(objApprovedHist);
            }
           // TempData["ViewHistoryGrd"] = objApprovedHist.ApprovalDetails;

            return Json(new { data = objApprovedHist.ApprovalDetails });


        }
        public async Task<JsonResult> onchnagemodule(int moduleid)
        {
            WorkFlowInbox WorkFlowInbox = new WorkFlowInbox();


            //objin.LocCode = objSession.LocationCode;
            //objin.CreatedBy = objSession.UserId;
            WorkFlowInbox.moduleid = moduleid;
            //objin.getlistBussinessobj(objin);
            AccessRights accessRights = new AccessRights();
            accessRights.ModuleId = WorkFlowInbox.moduleid;
            accessRights = await _IAccessRights.FetchSubmodule(accessRights);
            WorkFlowInbox.listbussinessobj = accessRights.lstsubmodules;
            return Json(WorkFlowInbox.listbussinessobj);

        }

        public JsonResult FetchApprovedHistoryGrd()
        {
            WorkFlowInbox objApprovedHist = new WorkFlowInbox();

            if (TempData["ViewHistoryGrd"] != null)
            {
                //objApprovedHist.ApprovalDetails = TempData.Peek("ViewHistoryGrd") as List<clsWorkFlowInbox>;
                TempData.Keep("ViewHistoryGrd");

            }
            return Json(new { data = objApprovedHist.ApprovalDetails });


        }

        public JsonResult FetchRejectedHistory(int RecordId, string trans_type)
        {
            WorkFlowInbox objApprovedHist = new WorkFlowInbox();

            if (RecordId != 0)
            {
                //objApprovedHist.LocCode = objSession.LocationCode;
                //objApprovedHist.CreatedBy = objSession.UserId;
                //objApprovedHist.TransactionId = Convert.ToInt32(RecordId);
                //objApprovedHist.sActionType = trans_type;
                //objApprovedHist.getRejectedHist(objApprovedHist);
            }
            return Json(new { data = objApprovedHist.ApprovalDetails });



        }





    }
}