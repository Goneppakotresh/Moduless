using IEMS_FrontApplications.Comman;
using IEMS_WEB.Interface.WorkFlow;
using IEMS_WEB.Models.WorkFlow;
using Microsoft.AspNetCore.Mvc;

namespace IEMS_WEB.Controllers.WorkFlow
{
    [SessionExpirationFilter]
    public class AccessRightsController : Controller
    {
        private readonly IAccessRights _IAccessRights;
        public AccessRightsController(IAccessRights iAccessRights)
        {
            _IAccessRights = iAccessRights;
        }
        // GET: AccessRights
        public async Task<ActionResult> AccessRights()
        {
            AccessRights accessRights = new AccessRights();
            AccessRightsResponseModel accessRightResponseMode = new AccessRightsResponseModel();
            accessRights = await _IAccessRights.GetDropDown(accessRights);
            return View(accessRights);
        }

        [HttpPost]
        public async Task<JsonResult> GetSubModule(string moduleid)
        {
            AccessRights accessrights = new AccessRights();
            accessrights.ModuleId = Convert.ToInt32(moduleid);
            accessrights = await _IAccessRights.FetchSubmodule(accessrights);
            return Json(accessrights.lstsubmodules);

        }

        /// <summary>
        /// This is for Check Access Rights of user.
        /// Raghavendra, 29.09.2023.
        /// </summary>
        /// <param name="BOCode"></param>
        /// <param name="RoleID"></param>
        /// <returns>lstCheckAccessResult in Json</returns>
        public async Task<JsonResult> CheckAccessRights(string BOCode) // ,string RoleID
        {
            AccessRights objCAR = new AccessRights();
            objCAR.ARBOCode = BOCode;
            objCAR.ARRoleID = "1";   //RoleID to be assign. 
            objCAR = await _IAccessRights.CheckAccessRights(objCAR);
            return Json(objCAR.lstCheckAccessRights);
        }

        public async Task<ActionResult> GetAccessrigtsdet(string moduleid, string submoduleid, string roleid)
        {
            List<AccessRights> lstaccessrights = new List<AccessRights>();
            AccessRights accessrights = new AccessRights();
            if (moduleid == "0" && submoduleid == "")
            {
                return Json(new { data = lstaccessrights });

            }
            else
            {
                accessrights.ModuleId = Convert.ToInt32(moduleid);
                accessrights.SubModuleId = Convert.ToInt32(submoduleid);
                accessrights.RoleId = Convert.ToInt32(roleid);
                accessrights = await _IAccessRights.GetAccessrightsdet(accessrights);
                return Json(new { data = accessrights.lstAccessrights });
            }

        }

        public async Task<ActionResult> SaveAccesrights(string moduleid, string submoduleid, string roleid, string checkedvales)
        {
            AccessRights accessrights = new AccessRights();
            accessrights.ModuleId = Convert.ToInt32(moduleid);
            accessrights.SubModuleId = Convert.ToInt32(submoduleid);
            accessrights.RoleId = Convert.ToInt32(roleid);
            accessrights.CheckedValues = checkedvales;
            accessrights = await _IAccessRights.SaveAccesrights(accessrights);
            return Json(new { data = accessrights.ResponseCode });

        }

    }
}