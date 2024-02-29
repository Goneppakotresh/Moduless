using IEMS_FrontApplications.Comman;
using IEMS_WEB.Comman;
using IEMS_WEB.Interface.WorkFlow;
using IEMS_WEB.Models;
using IEMS_WEB.Models.WorkFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using Nancy.Json;

namespace IEMS_WEB.Controllers.WorkFlow
{
    //[SessionExpirationFilter]
    public class WorkFlowRuleController : Controller
    {
        SessionDetails objSession;
        private void SetSessionValue()
        {
            objSession = HttpContext.Session.GetComplexData<SessionDetails>("SessionDetails");
        }
        private readonly IWorkFlow _iWorkFlow;
        private readonly IAccessRights _accessRights;
        public WorkFlowRuleController(IWorkFlow iWorkFlow, IAccessRights accessRights)
        {
            try
            {
                _iWorkFlow = iWorkFlow;
                _accessRights = accessRights;
            }
            catch (Exception) { }
        }
        // GET: WorkFlowRule
        [SessionExpirationFilter]
        public async Task<ActionResult> WorkFlowCreation(int WorkFlowId, string Type, int OldWorkFlowId = 0)
        {
            WorkFlowObject workFlowObject = new WorkFlowObject();

            try
            {
                //SetSessionValue();
                TempData.Remove("lstStageDetails");
                TempData.Remove("lstConditionBuilder");
                TempData.Remove("ConditionVal");
                TempData.Remove("hdnConditionVal");
                TempData.Remove("lstActionDetail");
                TempData.Remove("lstStageDetails");
                TempData.Remove("lstConditionBuilder");
                workFlowObject.WorkFlowObjectId = WorkFlowId;
                if (Type.ToUpper() == "V")
                {
                    workFlowObject.ApprovalStatus = 1;
                }
                workFlowObject.WorkFlowID = OldWorkFlowId;
                //workFlowObject = workFlowObject.fetchWorkFlowDetails(workFlowObject);
                workFlowObject = await _iWorkFlow.fetchWorkFlowDetails(workFlowObject);  //Workflow creation
                TempData.Put("lstStageDetails",workFlowObject.lstStageDetails);
                TempData.Put("lstConditionBuilder",workFlowObject.lstConditionBuilderDetails);
                //if (objSession != null)
                //{

                //    workFlowObject.CreatedBy = objSession.UserId;
                //    workFlowObject.CurrRoleId = objSession.RoleId;
                //}
            }
            catch (Exception ex)
            {

            }

            return View(workFlowObject);
        }



        public async Task<ActionResult> GetStageDetails(string StageKey, string category, int hdnStatus)
        {
            Stage objStageDet = new Stage();
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            WorkFlowDropDownModelOverride workFlowDropDownModelOverride = new WorkFlowDropDownModelOverride();
            WorkFlowDropDownModel workFlowDropDownModel = new WorkFlowDropDownModel();
            try
            {
                objStageDet.StageKey = Convert.ToInt32(StageKey);
                workFlowDropDownModel.DropDownType = "LOCATION_TYPE";
                ViewBag.lstLocationType = await _iWorkFlow.GetWFDropDown(workFlowDropDownModel);
                workFlowDropDownModelOverride = await _iWorkFlow.GetActionDet(workFlowDropDownModelOverride);
                if (category.ToUpper() == "START")
                {
                    objStageDet.StageType = "Start";
                    objStageDet.lstActionDet = workFlowDropDownModelOverride.DropDown.Where(x => x.ActionType.Trim() == "S").Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                }
                else
                {
                    objStageDet.StageType = "Intermediate";
                    objStageDet.lstActionDet = workFlowDropDownModelOverride.DropDown.Where(x => x.ActionType.Trim() != "S").Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
                }

                if (TempData["lstStageDetails"] != null)
                {
                    objWorkFlow.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                }
                TempData.Put("lstStageDetails", objWorkFlow.lstStageDetails);
                if (objWorkFlow.lstStageDetails.Where(x => x.StageKey == Convert.ToInt32(StageKey)).ToList().Count() > 0)
                {
                    foreach (Stage objStage in objWorkFlow.lstStageDetails.Where(x => x.StageKey == Convert.ToInt32(StageKey)).ToList())
                    {
                        objStageDet.Action = objStage.Action;
                        objStageDet.StageName = objStage.StageName;
                        objStageDet.StageId = objStage.StageId;
                        objStageDet.StageKey = objStage.StageKey;
                        objStageDet.StageDesc = objStage.StageDesc;
                        objStageDet.StageType = objStage.StageType;
                        if (objStageDet.StageType.ToUpper() == "S")
                        {
                            objStageDet.StageType = "Start";
                        }
                        if (objStageDet.StageType.ToUpper() == "I")
                        {
                            objStageDet.StageType = "Intermediate";
                        }
                        if (objStageDet.StageType.ToUpper() == "E")
                        {
                            objStageDet.StageType = "End";
                        }
                        objStageDet.LocationType = objStage.LocationType;
                        objStageDet.LocationPkId = objStage.LocationPkId;
                        objStageDet.RoleId = objStage.RoleId;
                        objStageDet.DeadlineType = objStage.DeadlineType;
                        objStageDet.DeadlineValue = objStage.DeadlineValue;
                        objStageDet.EnableEmail = objStage.EnableEmail;
                        objStageDet.EnableLog = objStage.EnableLog;
                        objStageDet.EnableSMS = objStage.EnableSMS;
                        //objStageDet.ApprovalStatus = hdnStatus;
                    }
                }
                if (objStageDet.LocationType == null || objStageDet.LocationType == "0")
                {
                    objStageDet.LocationType = "";
                }
                objStageDet.lstRole = await _iWorkFlow.GetRole(objStageDet.LocationType);
                objStageDet.lstLocationNames = await GetLocation(objStageDet.LocationType);
                objStageDet.lstDeadline.Add(new SelectListItem
                {
                    Text = "Select",
                    Value = "0"
                });

                objStageDet.lstDeadline.Add(new SelectListItem
                {
                    Text = "Hour",
                    Value = "H"
                });
                objStageDet.lstDeadline.Add(new SelectListItem
                {
                    Text = "Minutes",
                    Value = "M"
                });
                objStageDet.lstDeadline.Add(new SelectListItem
                {
                    Text = "Days",
                    Value = "D"
                });
                return PartialView("_StageDetails", objStageDet);
            }
            catch (Exception ex)
            {
                return Json(objStageDet);
            }

        }


        public async Task<ActionResult> WorkFlowView()
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            WorkFlowDropDownModel workFlowDropDownModel = new WorkFlowDropDownModel();
            workFlowDropDownModel.DropDownType = "MODULE";
            //SetSessionValue();
            //if (objSession != null)
            //{
            //objWorkFlow.CurrRoleId = objSession.RoleId;
            ViewBag.LstModule = await _iWorkFlow.GetWFDropDown(workFlowDropDownModel);
            workFlowDropDownModel.DropDownType = "COMPANY";
            ViewBag.LstCompany = await _iWorkFlow.GetWFDropDown(workFlowDropDownModel);
            workFlowDropDownModel.DropDownType = "ROLE";
            ViewBag.LstRole = await _iWorkFlow.GetWFDropDown(workFlowDropDownModel);
            //objWorkFlow.FetchPopUPDropDown();
            objWorkFlow.lstBusinessObject.Add(new SelectListItem { Value = "0", Text = "-ALL-", Selected = true });
            return View(objWorkFlow);
        }

        public async Task<JsonResult> FetchBusinessObject(string Module)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            WorkFlowDropDownModel workFlowDropDownModel = new WorkFlowDropDownModel();
            //SetSessionValue();
            //if (objSession != null)
            //{
            //objWorkFlow.CreatedBy = objSession.UserId;
            workFlowDropDownModel.DropDownType = "SUB_MODULE";
            objWorkFlow.lstBusinessObject = await _iWorkFlow.GetWFDropDown(workFlowDropDownModel);
            //}
            return Json(objWorkFlow.lstBusinessObject);
        }

        public async Task<ActionResult> LoadWorkFlowGrid(string Module, string BusinessObject)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            //SetSessionValue();
            //if (objSession != null)
            //{
            objWorkFlow.Module = Module;
            if (BusinessObject == null || BusinessObject == "")
            {
                BusinessObject = "0";
            }
            objWorkFlow.BObjId = Convert.ToInt32(BusinessObject);
            objWorkFlow = await _iWorkFlow.LoadWorkFlowGridDetails(objWorkFlow);
            //}

            return Json(new { data = objWorkFlow.lstWorkFlow });
        }


        public JsonResult AddStageDetails(string StageName, int stageKey, string StageType, string LocationType, string[] ActionId, string[] RoleId, string StageDesc, string DeadlineType, bool chkSMS, bool chklog, bool chkEMail, int DeadLine, int StageId, int LocationPkId)
        {
            WorkFlowObject objStageDet = new WorkFlowObject();
            try
            {
                objStageDet.lstStageDetails = new List<Stage>();
                if (TempData["lstStageDetails"] != null)
                {
                    objStageDet.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                }
                foreach (Stage ObjStage in objStageDet.lstStageDetails.Where(x => x.StageKey == Convert.ToInt32(stageKey)).ToList())
                {
                    objStageDet.lstStageDetails.Remove(ObjStage);

                }
                if (objStageDet.lstStageDetails.Where(x => x.StageName == StageName).ToList().Count() > 0)
                {

                    //TempData["lstStageDetails"] = objStageDet.lstStageDetails;
                    TempData.Put("lstStageDetails", objStageDet.lstStageDetails);

                    objStageDet.statusId = 0;
                    objStageDet.message = "Stage Name Already Exist";
                    return Json(objStageDet);
                }
                if (LocationType == null || LocationType == "0")
                {
                    LocationType = "";
                }
                objStageDet.lstStageDetails.Add(new Stage
                {
                    StageId = StageId,
                    StageKey = stageKey,
                    StageName = StageName,
                    StageDesc = StageDesc,
                    Action = ActionId,
                    RoleId = RoleId,
                    LocationType = LocationType,
                    StageType = StageType,
                    DeadlineType = DeadlineType,
                    EnableSMS = chkSMS,
                    EnableEmail = chkEMail,
                    EnableLog = chklog,
                    DeadlineValue = DeadLine,
                    LocationPkId = LocationPkId,

                });
                TempData.Put("lstStageDetails", objStageDet.lstStageDetails);
                objStageDet.statusId = 1;
                return Json(objStageDet);
            }
            catch (Exception ex)
            {
                return Json(objStageDet);
            }

        }



        public async Task<ActionResult> GetStageConditionalParam(int StageKey, string jsonval, string category, int BObjId, int WorkFlowObjectId, int hdnStatus)
        {
            Stage objStageDet = new Stage();
            WorkFlowObject objWorkFlowStage = new WorkFlowObject();
            WorkFlowDropDownModel workFlowDropDownModel = new WorkFlowDropDownModel();
            try
            {
               //var jsonString = JsonSerializer.Serialize();

                var serializer = new JavaScriptSerializer();
                dynamic dynObj = serializer.DeserializeObject(jsonval.Replace("\n", "").Trim());
                //dynamic dynObj = null;
                var deSerializedObj = JsonConvert.DeserializeObject(dynObj);
                var nodeDataArray = deSerializedObj["linkDataArray"];
                List<LinkNodeArray> lstKeyValue = new List<LinkNodeArray>();
                foreach (var node in nodeDataArray)
                {
                    lstKeyValue.Add(new LinkNodeArray
                    {
                        Nodefrom = Convert.ToInt32(node.from),
                        Nodeto = Convert.ToInt32(node.to)
                    });
                }

                objWorkFlowStage.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                TempData.Put("lstStageDetails", objWorkFlowStage.lstStageDetails);

                objStageDet.FromStageId = StageKey;



                if (objWorkFlowStage.lstStageDetails != null)
                {
                    if (objWorkFlowStage.lstStageDetails.Count > 0)
                    {
                        if (objWorkFlowStage.lstStageDetails.Where(x => x.StageKey == StageKey).ToList().Count == 0)
                        {
                            objStageDet.StatusId = 0;
                            objStageDet.message = "Please Provide the Stage Details";
                            objStageDet.StageId = StageKey;
                            return Json(objStageDet);
                        }
                    }
                    else
                    {
                        objStageDet.StatusId = 0;
                        objStageDet.StageId = StageKey;
                        objStageDet.message = "Please Provide the Stage Details";
                        return Json(objStageDet);
                    }

                }
                else
                {
                    objStageDet.StatusId = 0;
                    objStageDet.StageId = StageKey;
                    objStageDet.message = "Please Provide the Stage Details";
                    return Json(objStageDet);
                }


                objStageDet.FromStageName = objWorkFlowStage.lstStageDetails.Where(x => x.StageKey == StageKey).First().StageName;

                bool blCheckStageDetails = true;
                string Message = "";
                int StageKeyDet = 0;
                if (objWorkFlowStage.lstStageDetails != null)
                {
                    List<Stage> lstStageDetails = objWorkFlowStage.lstStageDetails.Where(x => x.StageKey == StageKey).ToList();
                    if (lstStageDetails.Count > 0)
                    {
                        List<SelectListItemOverRide> lstActionDetails = new List<SelectListItemOverRide>();
                        lstActionDetails = await _iWorkFlow.GetActionDet(lstStageDetails.Where(x => x.StageKey == StageKey).First().Action);

                        TempData.Put("lstActionDetail",lstActionDetails);
                        objStageDet.lstActionDet = lstActionDetails.Select(s => new SelectListItem { Value = s.Value, Text = s.Text }).ToList();

                        objStageDet.lstStage.Add(new SelectListItem
                        {
                            Text = "Select",
                            Value = "0"
                        });
                        foreach (LinkNodeArray node in lstKeyValue.Where(x => x.Nodefrom == Convert.ToInt32(StageKey)))
                        {
                            List<Stage> lStageDetails = objWorkFlowStage.lstStageDetails.Where(x => x.StageKey == node.Nodeto).ToList();
                            if (lStageDetails.Count > 0)
                            {
                                objStageDet.lstStage.Add(new SelectListItem
                                {
                                    Text = lStageDetails[0].StageName,
                                    Value = Convert.ToString(lStageDetails[0].StageKey)
                                });
                            }
                            else
                            {
                                blCheckStageDetails = false;
                                Message = "Please Provide the Stage Details";
                                StageKeyDet = node.Nodeto;
                                break;
                            }
                        }
                        if (blCheckStageDetails == false)
                        {
                            objStageDet.StatusId = 0;
                            objStageDet.message = Message;
                            objStageDet.StageId = StageKeyDet;
                            return Json(objStageDet);
                        }
                    }
                    else
                    {
                        objStageDet.StatusId = 0;
                        objStageDet.message = "The From Stage Details has been not provided";
                        return Json(objStageDet);
                    }
                }
                else
                {
                    objStageDet.StatusId = 0;
                    objStageDet.message = "The From Stage Details has been not provided";
                    return Json(objStageDet);
                }

                //objStageDet.lstProperties = objStageDet.GetPropertyDetail(WorkFlowObjectId);
                workFlowDropDownModel.DropDownType = "BO_PROPERY";
                objStageDet.lstProperties = await _iWorkFlow.GetWFDropDown(workFlowDropDownModel);
                //objStageDet.lstLogicalOperator = objStageDet.fetchLogicalOperation();
                workFlowDropDownModel.DropDownType = "LOGICAL_OPERATOR";
                objStageDet.lstLogicalOperator = await _iWorkFlow.GetWFDropDown(workFlowDropDownModel);
                objStageDet.StageKey = StageKey;
                objStageDet.BObjId = BObjId;
                objStageDet.WorkFlowObjectId = WorkFlowObjectId;
                objStageDet.lstStage.Add(new SelectListItem
                {
                    Text = "End",
                    Value = Convert.ToString(-999)
                });


                objStageDet.WorkflowDesignVal = jsonval;
                objStageDet.ApprovalStatus = hdnStatus;
                return PartialView("_ConditionalParam", objStageDet);
            }
            catch (Exception ex)
            {
                return Json(objStageDet);
            }
        }


        public async Task<List<SelectListItem>> GetLocation(string loctype)
        {
            Stage objStageDet = new Stage();
            try
            {
                objStageDet.lstLocationNames = await _iWorkFlow.FetchLoations(loctype);
                return objStageDet.lstLocationNames;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public JsonResult CheckNodeDetails(int Key)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            Stage objStageDet = new Stage();
            try
            {
                //SetSessionValue();
                //if (objSession != null)
                //{
                //    objWorkFlow.CreatedBy = objSession.UserId;
                //    objWorkFlow.CurrRoleId = objSession.RoleId;
                //    objWorkFlow.LocCode = objSession.LocationCode;

                if (TempData["lstConditionBuilder"] != null)
                {
                    objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstStageDetails");
                    TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                }
                if (TempData["lstStageDetails"] != null)
                {
                    objWorkFlow.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                    TempData.Put("lstStageDetails",objWorkFlow.lstStageDetails);

                }
                int iCount = objWorkFlow.lstConditionBuilderDetails.Where(x => x.ToStageId == Key).ToList().Count;
                if (iCount > 0)
                {
                    objStageDet.StatusId = 1;
                    //string StageName = "";
                    if (objWorkFlow.lstStageDetails.Where(x => x.StageKey == Key).ToList().Count > 0)
                    {
                        objStageDet.StageName = objWorkFlow.lstStageDetails.Where(x => x.StageKey == Key).ToList().First().StageName;
                    }
                    objStageDet.message = "Selected Stage " + objStageDet.StageName + " been used in the Condition Builder of the " + iCount;
                    return Json(objStageDet);
                }
                else
                {
                    objStageDet.StatusId = 0;
                    objStageDet.message = "Not Linked to any Stage";
                    return Json(objStageDet);
                }
                //}
            }
            catch (Exception ex)
            {
                objWorkFlow.statusId = -1;
                objWorkFlow.message = "Something Went Wrong,Please try again later";
            }
            return Json(objWorkFlow);
        }
        public JsonResult fetchStageOnActionDet(string ActionId, string StageValues, int StageKey, string WorkflowDesignVal)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            Stage objStageDet = new Stage();
            try
            {
                //SetSessionValue();
                //if (objSession != null)
                //{
                //    objWorkFlow.CreatedBy = objSession.UserId;
                //    objWorkFlow.CurrRoleId = objSession.RoleId;
                //    objWorkFlow.LocCode = objSession.LocationCode;

                List<SelectListItemOverRide> lstActionDet = new List<SelectListItemOverRide>();
                if (TempData["lstActionDetail"] != null)
                {
                    lstActionDet = TempData.Get<List<SelectListItemOverRide>>("lstActionDetail");
                    TempData.Put("lstActionDetail",lstActionDet);
                }

                var serializer = new JavaScriptSerializer();
                dynamic dynObj = serializer.DeserializeObject(WorkflowDesignVal.Replace("\n", "").Trim());
                //dynamic dynObj = null;
                var deSerializedObj = JsonConvert.DeserializeObject(dynObj);
                var nodeDataArray = deSerializedObj["linkDataArray"];
                List<LinkNodeArray> lstKeyValue = new List<LinkNodeArray>();
                foreach (var node in nodeDataArray)
                {
                    lstKeyValue.Add(new LinkNodeArray
                    {
                        Nodefrom = Convert.ToInt32(node.from),
                        Nodeto = Convert.ToInt32(node.to)
                    });
                }
                if (TempData["lstStageDetails"] != null)
                {
                    objWorkFlow.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                    TempData.Put("lstStageDetails", objWorkFlow.lstStageDetails);
                }
                if (lstActionDet.Where(x => x.Value == ActionId).ToList().Count > 0)
                {
                    string ActionType = lstActionDet.Where(x => x.Value == ActionId).ToList().First().ActionType;
                    objStageDet.lstStage.Add(new SelectListItem
                    {
                        Text = "Select",
                        Value = "0"
                    });

                    if (ActionType.ToUpper() == "F")
                    {
                        foreach (LinkNodeArray node in lstKeyValue.Where(x => x.Nodefrom == StageKey).ToList())
                        {
                            foreach (Stage objStage in objWorkFlow.lstStageDetails.Where(x => x.StageKey == node.Nodeto).ToList())
                            {
                                objStageDet.lstStage.Add(new SelectListItem
                                {
                                    Text = objStage.StageName,
                                    Value = Convert.ToString(objStage.StageKey)
                                });
                            }
                        }
                        objStageDet.lstStage.Add(new SelectListItem
                        {
                            Text = "End",
                            Value = Convert.ToString(-999)
                        });
                    }

                    objStageDet.lstStage = objStageDet.lstStage.OrderBy(x => x.Value).ToList();
                }
                objStageDet.StatusId = 1;
                //}
                return Json(objStageDet);
            }
            catch (Exception ex)
            {
                objStageDet.StatusId = -1;
                objStageDet.message = "Something Went Wrong,Please try again later";
                return Json(objStageDet);
            }
        }
        public JsonResult OnUncheckCondReq()
        {
            //SetSessionValue();
            TempData.Remove("ConditionVal");
            TempData.Remove("hdnConditionVal");
            return Json("1");
        }

        public JsonResult DeleteNode(int Key)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {

                //SetSessionValue();
                //if (objSession != null)
                //{
                //objWorkFlow.CreatedBy = objSession.UserId;
                //objWorkFlow.CurrRoleId = objSession.RoleId;
                //objWorkFlow.LocCode = objSession.LocationCode;
                if (TempData["lstConditionBuilder"] != null)
                {
                    objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstStageDetails");
                    foreach (Stage objStageDet in objWorkFlow.lstConditionBuilderDetails.FindAll(x => x.StageKey == Key).ToList())
                    {
                        objWorkFlow.lstConditionBuilderDetails.Remove(objStageDet);
                    }
                    foreach (Stage objStageDet in objWorkFlow.lstConditionBuilderDetails.Where(x => x.ToStageId == Key).ToList())
                    {
                        objWorkFlow.lstConditionBuilderDetails.Remove(objStageDet);
                    }
                    TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                }
                if (TempData["lstStageDetails"] != null)
                {
                    objWorkFlow.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                    foreach (Stage objStageDet in objWorkFlow.lstStageDetails.FindAll(x => x.StageKey == Key).ToList())
                    {
                        objWorkFlow.lstStageDetails.Remove(objStageDet);
                    }
                    TempData.Put("lstStageDetails",objWorkFlow.lstStageDetails);
                }
                objWorkFlow.statusId = 1;
                //}
                return Json(objWorkFlow.statusId);
            }
            catch (Exception ex)
            {
                return Json(objWorkFlow);
            }
        }
        public JsonResult GetConditionalBuilderDet(int StageKey)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {
                // var jsonResult = new JsonResult();
                TempData.Remove("hdnConditionVal");
                TempData.Remove("ConditionVal");
                if (TempData["lstConditionBuilder"] != null)
                {
                    objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstConditionBuilder");
                    TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                    objWorkFlow.lstConditionBuilderDetails = objWorkFlow.lstConditionBuilderDetails.Where(x => x.StageKey == StageKey).ToList();
                }
                // jsonResult = Json(new { data = objWorkFlow.lstConditionBuilderDetails });
                // jsonResult.MaxJsonLength = int.MaxValue;

                return Json(new { data = objWorkFlow.lstConditionBuilderDetails });
            }
            catch (Exception ex)
            {
                return Json(new { data = objWorkFlow.lstConditionBuilderDetails });
            }
        }

        public JsonResult AddCondPropDetails(string ConditionBuilderVal, string hdnConditionBuilderVal, string OperationType, int StageId, int StageKey, int SeriesNo)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {
                List<Tuple<string, string, int, int>> lstConditionVal = new List<Tuple<string, string, int, int>>();
                if (TempData["ConditionVal"] != null)
                {
                    lstConditionVal = TempData.Get<List<Tuple<string, string, int, int>>>("ConditionVal");
                }
                lstConditionVal = new List<Tuple<string, string, int, int>>(lstConditionVal);
                lstConditionVal.Add(new Tuple<string, string, int, int>(ConditionBuilderVal, OperationType, StageKey, SeriesNo));
                TempData.Put("ConditionVal", lstConditionVal);

                List<Tuple<string, string, int, int>> lsthdnConditionVal = new List<Tuple<string, string, int, int>>();
                if (TempData["hdnConditionVal"] != null)
                {
                    lsthdnConditionVal = TempData.Get<List<Tuple<string, string, int, int>>>("hdnConditionVal");
                }
                lsthdnConditionVal = new List<Tuple<string, string, int, int>>(lsthdnConditionVal);

                lsthdnConditionVal.Add(new Tuple<string, string, int, int>(hdnConditionBuilderVal, OperationType, StageKey, SeriesNo));
                TempData.Put("hdnConditionVal",lsthdnConditionVal);
                string ConditionBuilderExist = String.Join(" ", lstConditionVal.Select(t => t.Item1).ToArray());

                //list.Select(t => string.Format("[ '{0}', '{1}']", t.Item1, t.Item2))
                return Json(new { status = 1, ConditionBuilderExist = String.Join(" ", lstConditionVal.Select(t => t.Item1).ToArray()), hdnConditionBuilderExist = String.Join(" ", lsthdnConditionVal.Select(t => t.Item1).ToArray()) });
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, ConditionBuilderExist = "", hdnConditionBuilderExist = "" });
            }

        }

        public JsonResult EditCondPropDetails(int ValueSelected, int StageKey)
        {
            Stage objStageDet = new Stage();
            try
            {
                List<Tuple<string, string, int, int>> lstConditionVal = new List<Tuple<string, string, int, int>>();
                List<Tuple<string, string, int, int>> lsthdnConditionVal = new List<Tuple<string, string, int, int>>();
                WorkFlowObject objWorkFlow = new WorkFlowObject();

                if (TempData["lstConditionBuilder"] != null)
                {
                    objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstConditionBuilder");
                }
                TempData.Remove("hdnConditionVal");
                TempData.Remove("ConditionVal");
                if (objWorkFlow.lstConditionBuilderDetails.Count > 0)
                {
                    objStageDet = objWorkFlow.lstConditionBuilderDetails.Where(x => x.SeriesNo == ValueSelected && x.StageKey == StageKey).ToList().First();
                    TempData.Put("ConditionVal",objStageDet.lstConditionVal);
                    TempData.Put("hdnConditionVal",objStageDet.lsthdnConditionVal);
                }

                TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                objStageDet.StatusId = 1;
                return Json(objStageDet);
            }
            catch (Exception ex)
            {
                return Json(objStageDet);
            }
        }

        public JsonResult ProceedConditionBuilder(int StageKey)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {
                if (TempData["lstConditionBuilder"] != null)
                {
                    objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstConditionBuilder");
                }
                foreach (Stage objWorkflow in objWorkFlow.lstConditionBuilderDetails.Where(x => x.StageKey == StageKey).ToList())
                {
                    objWorkflow.StatusId = 1;
                }
                TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                return Json("1");

            }
            catch (Exception ex)
            {
                return Json("-1");
            }
        }

        public JsonResult DeleteCondPropDetails(int ValueSelected, int StageKey)
        {
            Stage objStageDet = new Stage();
            try
            {
                WorkFlowObject objWorkFlow = new WorkFlowObject();
                if (TempData["lstConditionBuilder"] != null)
                {
                    objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstConditionBuilder");
                }
                objStageDet = objWorkFlow.lstConditionBuilderDetails.Where(x => x.SeriesNo == ValueSelected).First();
                objWorkFlow.lstConditionBuilderDetails.Remove(objStageDet);
                TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                TempData.Remove("ConditionVal");
                TempData.Remove("hdnConditionVal");
                foreach (var Item in objStageDet.lstConditionVal.Where(x => x.Item3 == StageKey && x.Item4 == ValueSelected))
                {
                    objStageDet.lstConditionVal.Remove(Item);
                }
                foreach (var Item in objStageDet.lsthdnConditionVal.Where(x => x.Item3 == StageKey && x.Item4 == ValueSelected))
                {

                    objStageDet.lsthdnConditionVal.Remove(Item);
                }
            }
            catch (Exception ex)
            {
                return Json(objStageDet);
            }
            objStageDet.StatusId = 1;
            return Json(objStageDet);

        }

        public JsonResult UndoCondPropDetails(int StageId, int SeriesNo)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {
                Stage objStageDet = new Stage();
                List<Tuple<string, string, int, int>> lstConditionVal = new List<Tuple<string, string, int, int>>();
                if (TempData["ConditionVal"] != null)
                {
                    objStageDet.lstConditionVal = TempData.Get<List<Tuple<string, string, int, int>>>("ConditionVal");
                }
                objStageDet.lstConditionVal = new List<Tuple<string, string, int, int>>(objStageDet.lstConditionVal);
                lstConditionVal = objStageDet.lstConditionVal.Where(x => x.Item3 == StageId && x.Item4 == SeriesNo).ToList();

                var Item = lstConditionVal[lstConditionVal.Count - 1];



                objStageDet.lstConditionVal.RemoveAt(lstConditionVal.Count - 1);

                //objStageDet.lstConditionVal.Remove(Item);

                TempData["ConditionVal"] = objStageDet.lstConditionVal;

                List<Tuple<string, string, int, int>> lsthdnConditionVal = new List<Tuple<string, string, int, int>>();
                if (TempData["hdnConditionVal"] != null)
                {
                    objStageDet.lsthdnConditionVal = TempData.Get<List<Tuple<string, string, int, int>>>("hdnConditionVal");
                }
                objStageDet.lsthdnConditionVal = new List<Tuple<string, string, int, int>>(objStageDet.lsthdnConditionVal);

                lsthdnConditionVal = objStageDet.lsthdnConditionVal.Where(x => x.Item3 == StageId && x.Item4 == SeriesNo).ToList();

                var hdnItem = lsthdnConditionVal[lsthdnConditionVal.Count - 1];

                objStageDet.lsthdnConditionVal.RemoveAt(lsthdnConditionVal.Count - 1);

                //  objStageDet.lsthdnConditionVal.RemoveAt(lsthdnConditionVal.Count - 1);
                TempData.Put("hdnConditionVal",objStageDet.lsthdnConditionVal);
                return Json(new { status = 1, ConditionBuilderExist = String.Join(" ", objStageDet.lstConditionVal.Select(t => t.Item1).ToArray()), hdnConditionBuilderExist = String.Join(" ", objStageDet.lsthdnConditionVal.Select(t => t.Item1).ToArray()) });
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, ConditionBuilderExist = "", hdnConditionBuilderExist = "" });
            }
        }

        public JsonResult FreezeCondPropDetails(string OnAction, int ActionId, int FromStage_Id, int ToStage_Id, string FromStage_Name, string ToStage_Name, bool chkCondReq, int StageKey, int SeriesNo, int StageId)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {
                Stage objStageDet = new Stage();
                List<Tuple<string, string, int, int>> lstConditionVal = new List<Tuple<string, string, int, int>>();
                List<Tuple<string, string, int, int>> lsthdnConditionVal = new List<Tuple<string, string, int, int>>();
                if (TempData["ConditionVal"] != null)
                {
                    lstConditionVal = TempData.Get<List<Tuple<string, string, int, int> >>("ConditionVal");
                }
                if (TempData["hdnConditionVal"] != null)
                {
                    lsthdnConditionVal =TempData.Get<List<Tuple<string, string, int, int>>>("hdnConditionVal");
                }
                if (TempData["lstConditionBuilder"] != null)
                {
                    objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstConditionBuilder");
                }
                //if (TempData["FinalConditionVal"] != null)
                //{
                //    objStageDet.lstConditionVal = (List<Tuple<string, string, int, int>>)TempData["FinalConditionVal"];
                //}
                //if (TempData["FinalhdnConditionVal"] != null)
                //{
                //    objStageDet.lsthdnConditionVal = (List<Tuple<string, string, int, int>>)TempData["FinalhdnConditionVal"];
                //}

                //SeriesNo = objStageDet.lsthdnConditionVal.Select 
                var sequence = 0;
                if (SeriesNo == 0)
                {
                    if (objWorkFlow.lstConditionBuilderDetails.Count > 0)
                    {
                        SeriesNo = objWorkFlow.lstConditionBuilderDetails.OrderByDescending(item => item.SeriesNo).First().SeriesNo + 1;

                        objWorkFlow.lstStageDetails = objWorkFlow.lstConditionBuilderDetails.Where(x => x.StageKey == StageKey && x.chkCondReq == chkCondReq).ToList();
                        sequence = (objWorkFlow.lstStageDetails.Count > 0 ? objWorkFlow.lstStageDetails.Max(x => x.Sequence) + 1 : 1);


                    }
                    else
                    {
                        SeriesNo = SeriesNo + 1;
                        sequence = 1;
                    }
                }
                else
                {
                    Stage obj = objWorkFlow.lstConditionBuilderDetails.Where(x => x.SeriesNo == SeriesNo && x.StageKey == StageKey).ToList().First();
                    sequence = objWorkFlow.lstConditionBuilderDetails.Where(x => x.SeriesNo == Convert.ToInt32(SeriesNo) && x.StageKey == StageKey).ToList()[0].Sequence;

                    objWorkFlow.lstConditionBuilderDetails.Remove(obj);
                }
                for (int i = 0; i < lstConditionVal.Count; i++)
                {
                    var item1 = lstConditionVal[i];

                    var item2 = lsthdnConditionVal[i];

                    objStageDet.lstConditionVal.Add(new Tuple<string, string, int, int>(item1.Item1, item1.Item2, item1.Item3, SeriesNo));

                    objStageDet.lsthdnConditionVal.Add(new Tuple<string, string, int, int>(item2.Item1, item2.Item2, item2.Item3, SeriesNo));
                }

                if (chkCondReq == false)
                {

                    var lstFilteredDetails = objWorkFlow.lstConditionBuilderDetails.Where(x => x.chkCondReq == false).ToList();
                    if (lstFilteredDetails.Count > 0)
                    {
                        int iCount = lstFilteredDetails.Where(x => x.ToStageId == ToStage_Id && x.ActionId == ActionId && x.StageKey == StageKey).ToList().Count();
                        if (iCount > 0)
                        {
                            TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                            return Json("0");

                        }

                        int jCount = lstFilteredDetails.Where(x => x.ActionId == ActionId && x.StageKey == StageKey).ToList().Count();
                        if (jCount > 0)
                        {
                            TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                            return Json("2");
                        }
                    }
                }
                objWorkFlow.lstConditionBuilderDetails.Add(new Stage
                {
                    ActionId = ActionId,
                    ActionName = OnAction,
                    FromStageId = FromStage_Id,
                    FromStageName = FromStage_Name,
                    ToStageId = ToStage_Id,
                    ToStageName = ToStage_Name,
                    StageKey = StageKey,
                    lstConditionVal = objStageDet.lstConditionVal,
                    lsthdnConditionVal = objStageDet.lsthdnConditionVal,
                    hdnConditionBuilder = String.Join(" ", objStageDet.lsthdnConditionVal.Select(t => t.Item1).ToArray()),
                    ConditionBuilder = String.Join(" ", objStageDet.lstConditionVal.Select(t => t.Item1).ToArray()),
                    SeriesNo = SeriesNo,
                    chkCondReq = chkCondReq,
                    StageId = StageId,
                    Sequence = Convert.ToInt32(sequence)
                });

                TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                //TempData["FinalConditionVal"] = objStageDet.lstConditionVal;
                //TempData["FinalhdnConditionVal"] = objStageDet.lsthdnConditionVal;
                TempData.Remove("ConditionVal");
                TempData.Remove("hdnConditionVal");
            }
            catch (Exception ex)
            {
                return Json("-1");
            }
            return Json("1");
        }

        public async Task<JsonResult> GetRoleDetails(string LocationType)
        {
            Stage objStageDet = new Stage();
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {
                objStageDet.lstRole = await _iWorkFlow.GetRole(LocationType);
                objStageDet.lstLocationNames = await GetLocation(LocationType);
                if (TempData["lstStageDetails"] != null)
                {
                    objWorkFlow.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                    TempData.Put("lstStageDetails", objWorkFlow.lstStageDetails);
                }
                objStageDet.StatusId = 1;
                return Json(objStageDet);

            }
            catch (Exception ex)
            {
                return Json(objStageDet);
            }
        }

        public async Task<JsonResult> fetchPropertieDet(string ValueSelected)
        {
            Stage objStageDet = new Stage();
            try
            {
                objStageDet = await _iWorkFlow.FetchBOPropDetails(Convert.ToInt32(ValueSelected));

                return Json(objStageDet);
            }
            catch (Exception ex)
            {

                return Json(objStageDet);
            }

        }

        public JsonResult GetStageRoleDetails(string StageKey)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();

            try
            {
                if (TempData["lstStageDetails"] != null)
                {
                    objWorkFlow.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                    TempData.Put("lstStageDetails",objWorkFlow.lstStageDetails);
                    objWorkFlow.lstStageDetails = objWorkFlow.lstStageDetails.Where(x => x.StageKey == Convert.ToInt32(StageKey)).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return Json(new { data = objWorkFlow.lstStageDetails });
        }

        public JsonResult AddOrUpdateSequence(string seriesno, string sequenceid, string StageKey)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {
                if (TempData["lstConditionBuilder"] != null)
                {
                    objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstConditionBuilder");
                }
                var isSequenceRepeated = objWorkFlow.lstConditionBuilderDetails.Exists(x => x.SeriesNo != Convert.ToInt32(seriesno) && x.StageKey == Convert.ToInt32(StageKey) && x.Sequence == Convert.ToInt32(sequenceid) && Convert.ToInt32(sequenceid) != 0);
                if (isSequenceRepeated == true)
                {
                    TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                    objWorkFlow.statusId = -2;
                    return Json(objWorkFlow);
                }
                foreach (Stage obj in objWorkFlow.lstConditionBuilderDetails.Where(x => x.SeriesNo == Convert.ToInt32(seriesno)).ToList())
                {
                    obj.Sequence = Convert.ToInt32(sequenceid);
                }

                TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                objWorkFlow.statusId = 1;

            }
            catch (Exception ex)
            {
                return Json(objWorkFlow);
            }
            return Json(objWorkFlow);

        }

        public async Task<JsonResult> EvaluateCondPropDetails(string ConditionBuilderVal, string hdnConditionBuilderVal, int bObjId)
        {
            Stage objStageDet = new Stage();
            try
            {

                bool ConditionalBuilder = await _iWorkFlow.fetchConditionalBuilder(ConditionBuilderVal, hdnConditionBuilderVal, bObjId);
                if (ConditionalBuilder)
                {
                    return Json(1);
                }
                else
                {
                    return Json(0);
                }
            }
            catch (Exception ex)
            {
                return Json(-1);
            }

        }

        public async Task<ActionResult> FetchDataForUpdate(string id)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            // ClsWorkFlow objclsWorkFlow = objWorkFlow;
            try
            {
                //SetSessionValue();
                //if (objSession != null)
                ////{
                objWorkFlow.WorkFlowID = Convert.ToInt32(id);
                objWorkFlow = await _iWorkFlow.LoadWorkFlowData(id);
                //}
            }
            catch (Exception ex)
            {
                objWorkFlow.statusId = -1;
                objWorkFlow.message = "Something Went Wrong,Please try again later";
            }
            return Json(objWorkFlow);
        }

        //Come From WorkFlowView
        public async Task<ActionResult> SaveUpdateWorkFlowDetails(string Company, int BoNAmeID, string WFlowName, string WFlowDesc, string Role, string StartDate, string EndDate, int WorkFlowID, string isValid)
        {
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {
                //SetSessionValue();
                //if (objSession != null)
                //{
                objWorkFlow.Company = Company;
                objWorkFlow.BObjId = BoNAmeID;
                objWorkFlow.WorkFlowName = WFlowName;
                objWorkFlow.WorkFlowDescription = WFlowDesc;
                objWorkFlow.WorkFlowHeadId = Role;
                objWorkFlow.StartDate = StartDate;
                objWorkFlow.WorkFlowID = WorkFlowID;
                objWorkFlow.sIsValid = isValid;
                objWorkFlow.EndDate = EndDate;
                //objWorkFlow.CreatedBy = objSession.UserId;
                objWorkFlow = await _iWorkFlow.SaveupdateWorkFlowDetails(objWorkFlow);
                //}
            }
            catch (Exception ex)
            {
                objWorkFlow.statusId = -1;
                objWorkFlow.message = "Something Went Wrong,Please try again later";
            }
            return Json(objWorkFlow);
        }
        public async Task<JsonResult> SaveWorkFlowDraftDetails(string hdnOutPutConditionBuilder, int bObjId, int WorkFlowObjectId)
        {
            WorkFlowSaveRequest objWorkFlow = new WorkFlowSaveRequest();
            try
            {
                //SetSessionValue();
                //if (objSession != null)
                //{
                //    objWorkFlow.CreatedBy = objSession.UserId;
                //    objWorkFlow.CurrRoleId = objSession.RoleId;
                //    objWorkFlow.LocCode = objSession.LocationCode;
                var serializer = new JavaScriptSerializer();
                dynamic dynObj = serializer.DeserializeObject(hdnOutPutConditionBuilder.Replace("\n", "").Trim());
                //dynamic dynObj = null;
               var deSerializedObj = JsonConvert.DeserializeObject(dynObj);
                var nodeDataArray = deSerializedObj["nodeDataArray"];
                if (nodeDataArray.Count == 0)
                {
                    objWorkFlow.StatusId = 0;
                    objWorkFlow.message = "WorkFlow has not been designed to Save in Draft";
                    return Json(objWorkFlow);
                }
                if (TempData["lstStageDetails"] != null)
                {
                    objWorkFlow.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                    TempData.Put("lstStageDetails", objWorkFlow.lstStageDetails);

                }
                if (TempData["lstConditionBuilder"] != null)
                {
                    objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstConditionBuilder");
                    TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                }

                objWorkFlow.lstConditionBuilderDetails = objWorkFlow.lstConditionBuilderDetails.Where(x => x.StatusId == 1).ToList();
                objWorkFlow.SaveToDraft = 1;
                objWorkFlow.WorkFlowObjectId = WorkFlowObjectId;
                objWorkFlow.BObjId = bObjId;
                objWorkFlow.OutPutConditionBuilder = hdnOutPutConditionBuilder;
                objWorkFlow = await _iWorkFlow.SaveUpdateWFConfigurationDetails(objWorkFlow);
                objWorkFlow.StatusId = 1;
                objWorkFlow.message = "Workflow Design has been Saved to Draft";


                //}
                return Json(objWorkFlow);
            }
            catch (Exception ex)
            {
                objWorkFlow.StatusId = -1;
                objWorkFlow.message = "Something Went Wrong,Please try again later";
                return Json(objWorkFlow);
            }

        }

        public async Task<JsonResult> SaveWorkFlowDetails(string hdnOutPutConditionBuilder, int bObjId, int WorkFlowObjectId)
        {
            WorkFlowSaveRequest objWorkFlow = new WorkFlowSaveRequest();
            try
            {
                //SetSessionValue();
                //if (objSession != null)
                //{
                //    objWorkFlow.CreatedBy = objSession.UserId;
                //    objWorkFlow.CurrRoleId = objSession.RoleId;
                //    objWorkFlow.LocCode = objSession.LocationCode;
                // var serializer = new JavaScriptSerializer();
                List<Tuple<int, string>> NodeArray = new List<Tuple<int, string>>();
                if (hdnOutPutConditionBuilder != "")
                {
                    var serializer = new JavaScriptSerializer();
                    dynamic dynObj = serializer.DeserializeObject(hdnOutPutConditionBuilder.Replace("\n", "").Trim());
                    //dynamic dynObj = null;
                    var deSerializedObj = JsonConvert.DeserializeObject(dynObj);
                    var nodeDataArray = deSerializedObj["nodeDataArray"];
                    if (nodeDataArray.Count == 0)
                    {
                        objWorkFlow.StatusId = 0;
                        objWorkFlow.WorkFlowID = WorkFlowObjectId;
                        objWorkFlow.message = "WorkFlow has not been designed to proceeed";
                        return Json(objWorkFlow);
                    }
                    bool blStartStage = false;
                    foreach (var node in nodeDataArray)
                    {
                        NodeArray.Add(new Tuple<int, string>(Convert.ToInt32(node.key.Value), Convert.ToString(node.category.Value).ToString().ToUpper()));
                        if (Convert.ToString(node.category).ToString().ToUpper() == "START")
                        {
                            blStartStage = true;
                        }
                    }
                    if (blStartStage == false)
                    {
                        objWorkFlow.StatusId = 0;
                        objWorkFlow.WorkFlowID = WorkFlowObjectId;
                        objWorkFlow.message = "WorkFlow should have a Start Stage";
                        return Json(objWorkFlow);
                    }

                    var LinkDataArray = deSerializedObj["linkDataArray"];
                    if (LinkDataArray.Count == 0)
                    {
                        objWorkFlow.StatusId = 0;
                        objWorkFlow.WorkFlowID = WorkFlowObjectId;

                        objWorkFlow.message = "Stage are not Connected,Please Provide a Connection to Proceed Workflow";
                        return Json(objWorkFlow);
                    }
                    if (TempData["lstStageDetails"] != null)
                    {
                        objWorkFlow.lstStageDetails = TempData.Get<List<Stage>>("lstStageDetails");
                        TempData.Put("lstStageDetails",objWorkFlow.lstStageDetails);

                    }
                    if (TempData["lstConditionBuilder"] != null)
                    {
                        objWorkFlow.lstConditionBuilderDetails = TempData.Get<List<Stage>>("lstConditionBuilder");
                        TempData.Put("lstConditionBuilder",objWorkFlow.lstConditionBuilderDetails);
                    }
                    objWorkFlow.lstConditionBuilderDetails = objWorkFlow.lstConditionBuilderDetails.Where(x => x.StatusId == 1).ToList();
                    foreach (var node in nodeDataArray)
                    {
                        int FromNode = node.key;
                        if (objWorkFlow.lstStageDetails.Where(x => x.StageKey == FromNode).ToList().Count > 0)
                        {
                            foreach (Stage objStage in objWorkFlow.lstStageDetails.Where(x => x.StageKey == FromNode).ToList())
                            {
                                if (objWorkFlow.lstConditionBuilderDetails.Where(x => x.StageKey == objStage.StageKey).ToList().Count > 0)
                                {
                                    List<Stage> lstConditionBuilderVal = objWorkFlow.lstConditionBuilderDetails.Where(x => x.StageKey == objStage.StageKey).ToList();

                                    string[] sActionArrayVal = objStage.Action;
                                    foreach (string Action in sActionArrayVal)
                                    {
                                        if (Action.Split('~')[1] == "F")
                                        {
                                            if (lstConditionBuilderVal.Where(x => x.ActionId == Convert.ToInt32(Action.Split('~')[0]) && x.ConditionBuilder.Trim() == "").ToList().Count == 0)
                                            {

                                                objWorkFlow.StatusId = 2;
                                                objWorkFlow.WorkFlowID = WorkFlowObjectId;
                                                objWorkFlow.CurrStageId = FromNode;
                                                objWorkFlow.message = "Default Condition Builder Details has been Not Provided in the Stage " + objStage.StageName + " for Action : " + Action;
                                                return Json(objWorkFlow);
                                            }
                                            if (lstConditionBuilderVal.Where(x => x.ActionId == Convert.ToInt32(Action.Split('~')[0])).ToList().Count == 0)
                                            {

                                                objWorkFlow.StatusId = 2;
                                                objWorkFlow.WorkFlowID = WorkFlowObjectId;
                                                objWorkFlow.CurrStageId = FromNode;
                                                objWorkFlow.message = "Condition Builder Details has been Not Provided in the Stage " + objStage.StageName + " for Action : " + Action;
                                                return Json(objWorkFlow);
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    objWorkFlow.StatusId = 2;
                                    objWorkFlow.CurrStageId = objStage.StageKey;
                                    objWorkFlow.WorkFlowID = WorkFlowObjectId;
                                    objWorkFlow.message = "Condition Builder has been Not Provided for Stage " + objStage.StageName;
                                    return Json(objWorkFlow);
                                }
                            }
                        }
                        else
                        {
                            objWorkFlow.StatusId = 2;
                            objWorkFlow.CurrStageId = FromNode;
                            objWorkFlow.WorkFlowID = WorkFlowObjectId;
                            objWorkFlow.message = "Stage Details has been Not Provided for Stage";
                            return Json(objWorkFlow);
                        }
                    }
                }
                else
                {
                    objWorkFlow.StatusId = 0;
                    objWorkFlow.WorkFlowID = WorkFlowObjectId;
                    objWorkFlow.message = "WorkFlow has been not designed to proceeed";
                    return Json(objWorkFlow);
                }
                objWorkFlow.WorkFlowObjectId = WorkFlowObjectId;
                objWorkFlow.BObjId = bObjId;
                objWorkFlow.OutPutConditionBuilder = hdnOutPutConditionBuilder;

                if (NodeArray.Count > 0)
                {
                    foreach (var Item in NodeArray)
                    {
                        if (Item.Item2.ToUpper() != "START")
                        {
                            if (objWorkFlow.lstConditionBuilderDetails.Where(X => X.ToStageId.Equals(Item.Item1)).ToList().Count == 0)
                            {
                                var StageName = objWorkFlow.lstStageDetails.Where(x => x.StageKey == Convert.ToInt32(Item.Item1)).ToList().First().StageName;
                                objWorkFlow.StatusId = 2;
                                objWorkFlow.WorkFlowID = WorkFlowObjectId;
                                objWorkFlow.message = "For the Stage " + StageName + " there is no Condition Builder provided";
                                objWorkFlow.CurrStageId = Convert.ToInt32(Item.Item1);
                                return Json(objWorkFlow);
                            }
                        }
                    }
                }
                objWorkFlow.SaveToDraft = 0;
                objWorkFlow = await _iWorkFlow.SaveUpdateWFConfigurationDetails(objWorkFlow);
                objWorkFlow.StatusId = 1;
                objWorkFlow.WorkFlowID = WorkFlowObjectId;
                objWorkFlow.message = "Workflow Created Successfully";
                return Json(objWorkFlow);
                //}
                return Json(objWorkFlow);
            }
            catch (Exception ex)
            {
                objWorkFlow.StatusId = -1;
                objWorkFlow.message = "Something Went Wrong,Please try again later";
                return Json(objWorkFlow);
            }
        }

        public async Task<ActionResult> SetDefaultWorkFlow(string WorkFlowID, string Businessid, string BusinessObjectName)
        {
            //  Module: Module
            WorkFlowObject objWorkFlow = new WorkFlowObject();
            try
            {
                //updated by 
                //SetSessionValue();
                //if (objSession != null)
                //{
                //objWorkFlow.CreatedBy = objSession.UserId;
                objWorkFlow.WorkFlowID = Convert.ToInt32(WorkFlowID);
                objWorkFlow.BObjId = Convert.ToInt32(Businessid);

                objWorkFlow = await _iWorkFlow.SetDefaultStatusWorkFlow(objWorkFlow);
                objWorkFlow.BusinessObjectName = BusinessObjectName;
                //}
            }
            catch (Exception ex)
            {

            }
            return Json(objWorkFlow);
        }
        //Fetch Access Rights
        [Route("WorkFlowRule/FetchActionDetails")]
        [HttpGet]
        public async Task<JsonResult> FetchActionDetails(string FormCode, string StageType, int StageId, int RoleId, string RoleType, int StatusId = 0)
        {
            AccessRights objActionItems = new AccessRights();
            try
            {
                objActionItems.CurrRoleId = RoleId;
                objActionItems.CurrRoleType = RoleType;
                objActionItems.FormCode = FormCode;
                objActionItems.StageType = StageType;
                objActionItems.CurrStageId = StageId;
                objActionItems.pkid = StatusId;
                objActionItems.ActionTypes = await _accessRights.GetActionItemDetails(objActionItems);
                objActionItems.StatusId = 1;
                return Json(objActionItems);
            }
            catch (Exception ex)
            {
                return Json(objActionItems);
            }
        }


        public async Task<JsonResult> UpdateWorkFlowCancelDet(int WorkFlowObjectId, int TransactionId = 0, string FormCode = "")
        {
            SetSessionValue();
            WorkFlowObject objApprovalParam = new WorkFlowObject();
            //GenPVTrans objApprovalParam = new GenPVTrans();
            try
            {

                objApprovalParam.ApprovedBy = objSession.UserID;
                objApprovalParam.WorkFlowObjectId = WorkFlowObjectId;
                objApprovalParam.FormCode = FormCode;
                objApprovalParam.TransactionId = TransactionId;
                //objApprovalParam.ApprovedLocCode = objSession.LocationCode;
                objApprovalParam = await _iWorkFlow.WorkFlowCancelDet(objApprovalParam);
                return Json(objApprovalParam);

            }
            catch (Exception ex)
            {
                return Json(objApprovalParam);
            }
        }


    }

}