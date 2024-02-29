using IEMS_WEB.Interface;
using IEMS_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IEMS_WEB.Controllers
{
    public class DropDownController : Controller
    {
        private readonly IDropDownList _dropDownList;
        public DropDownController(IDropDownList DropDownList)
        {
            _dropDownList = DropDownList;
        }
        [HttpGet]
        public async Task<ActionResult> SelectDropDown(int ParentID = 0, string DropDownType = "")
        {
            List<SelectListItem> itm = new List<SelectListItem>();
            try
            {
                DropDownModel ObjDd = new DropDownModel();
                ObjDd.DropDownType = DropDownType;
                ObjDd.ParentID = ParentID;
                itm = await _dropDownList.GetDropDown(ObjDd);

            }
            catch (Exception ex)
            {

            }
            return Json(itm);
        }


        [HttpPost]
        public async Task<ActionResult> GetRouteListDropDown(RouteDetailRequest reqModel)
        {
            List<SelectListItem> itm = new List<SelectListItem>();
            try
            {
               
                itm = await _dropDownList.GetRouteMasterDropDown(reqModel);

            }
            catch (Exception ex)
            {

            }
            return Json(itm);
        }
    }
}
