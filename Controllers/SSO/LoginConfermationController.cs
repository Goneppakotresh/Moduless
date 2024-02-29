using IEMS_WEB.Areas.DepartmentServ.Interface;
using IEMS_WEB.Areas.DepartmentServ.Model.Response;
using IEMS_WEB.Areas.OtherLicensee.Interface;
using IEMS_WEB.Areas.SupplierInvoice.Model;
using IEMS_WEB.Comman;
using IEMS_WEB.Interface;
using IEMS_WEB.Interface.SSO;
using IEMS_WEB.Models;
using IEMS_WEB.Models.PublicServices.Request;
using IEMS_WEB.Models.PublicServices.Response;
using IEMS_WEB.Models.RequestModel;
using IEMS_WEB.Models.ResponseModel;
using IEMS_WEB.Models.SSO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Claims;

namespace IEMS_FrontApplications.Controllers
{

    public class LoginConfermationController : Controller
    {
        // GET: LoginConfermation
        private readonly ISSO _iSSORepo;
        private readonly IMenu _iMenu;
        private readonly CustomIDataProtection _protector;
        public IDepartment _department;
        SessionDetails objSession;
        private void SetSessionValue()
        {
            objSession = HttpContext.Session.GetComplexData<SessionDetails>("SessionDetails");
        }
        public LoginConfermationController(ISSO iSSORepo, IMenu iMenu, CustomIDataProtection protector, IDepartment department)
        {
            _iSSORepo = iSSORepo;
            _iMenu = iMenu;
            _protector = protector;
            _department = department;
        }
        [HttpGet]
        public ActionResult TEST()
        {

            return View();
        }
        //  [Route("LoginConfermation")]
        // [HttpPost]
        [HttpGet]
        public async Task<ActionResult> Index(string SSOToken = "", string SSOURL = "")
        {
            // TempData.Remove("SSOToken");
            //var SSOToken = Request.Query["userdetails"];
            bool SSOTokenExists = false;
            //if (Request != null && Request.Form != null)
            //{
            //    //SSOTokenExists = Request.Form.TryGetValue("userdetails", out var TokenfromSSO);
            //   // SSOToken = TokenfromSSO;
            //}

            #region Static Code added by Rajveer

            if (!string.IsNullOrEmpty(SSOToken))
            {
                SSOTokenExists = true;
            }
            #endregion
            if (SSOTokenExists)
            {
                // Session["SSOToken"] = SSOToken;
                HttpContext.Session.SetComplexData("SSOToken", SSOToken);
                // TempData["SSOToken"] = SSOToken;

                TempData.Put("SSOToken", Convert.ToString(SSOToken));
                try
                {
                    // string decryptData = EncodingDecoding.Decrypt(SSODetail, "E-m!tr@2016");
                    SSO_API_Token reqModel = new SSO_API_Token();
                    SSO_Token_Detail Resmodal = new SSO_Token_Detail();
                    SessionDetails objSession = new SessionDetails();
                    reqModel.API_Type = "/SSOREST/GetTokenDetailNewJson";
                    reqModel.Token = SSOToken;
                    //  Resmodal = await _iSSORepo.SSOTokenDetail(reqModel);
                    //Resmodal.sAMAccountName = "5466655522";
                    Resmodal.Status = 0;
                    if (Resmodal != null && Resmodal.Status == 1)
                    {

                        SSO_Response Responsemodel = new SSO_Response();
                        SSO_Request Requestmodel = new SSO_Request();
                        Requestmodel.SSO_ID = Resmodal.sAMAccountName;
                        // Requestmodel.SSO_ID = "JSHARMA.DOIT";
                        Responsemodel = await _iSSORepo.SSOLogin(Requestmodel);
                        Responsemodel.Token = SSOToken;

                        if (Responsemodel != null && Responsemodel.Status == 1)
                        {
                            MenuRequestModel menuMaster = new MenuRequestModel();

                            UserRequestModel reqLoginModel = new UserRequestModel();



                            #region claim bases authentication by Rajveer
                            //var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, Responsemodel.UserName, Responsemodel.RoleName);
                            // Adds the values to the claims we create.
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Id, Responsemodel.UserID.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Name, Responsemodel.UserName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Job, Responsemodel.RoleName));

                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.UserName, Responsemodel.RoleName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.UserId, Responsemodel.UserID.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.RoleName, Responsemodel.RoleName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.RoleId, Responsemodel.RoleId.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.RegistrationNo, Responsemodel.RegistrationNo));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Token, Responsemodel.Token));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LicenseeCode, Responsemodel.LicenseeCode));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LocationCode, Responsemodel.LocationCode.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Message, Responsemodel.Message));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Status, Responsemodel.Status.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LocationName, Responsemodel.LocationName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.SSO_ID, Responsemodel.SSO_ID));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LocationType, Responsemodel.LocationType));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.FirstName, Responsemodel.FirstName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Email, Responsemodel.Email));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.MobileNo, Responsemodel.MobileNo));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.UserRefID, Responsemodel.UserRefId.ToString()));



                            // Creates the context of user authentication.
                            var principal = new ClaimsPrincipal(identity);
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                                new AuthenticationProperties
                                {
                                    IsPersistent = true
                                });


                            #endregion


                            //UserResponseModel ResSSOmodel = new UserResponseModel();
                            //reqLoginModel.UserName = Responsemodel.UserName;
                            // ResSSOmodel = await _iSSORepo.LoginwithSSO(reqLoginModel);
                            //if (ResSSOmodel != null && ResSSOmodel.Status == 1)
                            // {
                            objSession = SetSession(Responsemodel);
                            objSession.IsSSOLogin = 1;
                            HttpContext.Session.SetComplexData("SessionDetails", objSession);

                            menuMaster.RoleId = Convert.ToInt32(Responsemodel.RoleId);
                            MenuResponseModel menuResponse = await _iMenu.MenuDetails(menuMaster);
                            HttpContext.Session.SetComplexData("MenuList", menuResponse.LstMenu);
                            HttpContext.Session.SetComplexData("RoleId", Convert.ToInt32(Responsemodel.RoleId));
                            HttpContext.Session.SetComplexData("UserId", Convert.ToInt32(Responsemodel.UserID));
                            HttpContext.Session.SetComplexData("UserRefId", Convert.ToInt32(Responsemodel.LicenseeCode));
                            HttpContext.Session.SetComplexData("LocationType", Responsemodel.LocationType.Trim());
                            HttpContext.Session.SetComplexData("LocationCode", Convert.ToInt32(Responsemodel.LocationCode));
                            HttpContext.Session.SetComplexData("MobileNumber", Responsemodel.MobileNo);
                            HttpContext.Session.SetComplexData("IsSSOLogin", objSession.IsSSOLogin);

                            //Session["MenuList"] = menuMaster.LstMenu;
                            //Session["UserName"] = Responsemodel.UserName;
                            //Session["UserId"] = Responsemodel.UserID;
                            //Session["RoleId"] = Responsemodel.RoleId;
                            //Session["RegistrationNo"] = Responsemodel.RegistrationNo;
                            //Session["IsSSOLogin"] = objSession.IsSSOLogin;

                            //Session["SSOToken"] = SSOToken;

                            ViewBag.token = "My Token is" + Responsemodel.Message;

                            if (Responsemodel.RoleId == 1)
                            {
                                return RedirectToAction("ImportBidders", "ImportBidders", new { Area = "RetailOff" });
                            }
                            else
                            {
                                return RedirectToAction("Dashboard", "Login");
                            }
                        }
                        else
                        {
                            return RedirectToAction("BackToSSO", "LoginConfermation");

                        }

                        //}
                    }
                    else
                    {
                        #region Get Citizen Role by Rajveer

                        return RedirectToAction("PublicServices", "LoginConfermation", new { token = SSOToken });
                        #endregion
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");

            }
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> Index()
        {
            // TempData.Remove("SSOToken");
            //var SSOToken = Request.Query["userdetails"];
            string SSOToken = "";
            bool SSOTokenExists = false;
            if (Request != null && Request.Form != null)
            {
                SSOTokenExists = Request.Form.TryGetValue("userdetails", out var TokenfromSSO);
                SSOToken = TokenfromSSO;
            }

            #region Static Code added by Rajveer

            if (!string.IsNullOrEmpty(SSOToken))
            {
                SSOTokenExists = true;
            }
            #endregion
            if (SSOTokenExists)
            {
                // Session["SSOToken"] = SSOToken;
                HttpContext.Session.SetComplexData("SSOToken", SSOToken);
                // TempData["SSOToken"] = SSOToken;

                TempData.Put("SSOToken", Convert.ToString(SSOToken));
                try
                {
                    // string decryptData = EncodingDecoding.Decrypt(SSODetail, "E-m!tr@2016");
                    SSO_API_Token reqModel = new SSO_API_Token();
                    SSO_Token_Detail Resmodal = new SSO_Token_Detail();
                    SessionDetails objSession = new SessionDetails();
                    reqModel.API_Type = "/SSOREST/GetTokenDetailNewJson";
                    reqModel.Token = SSOToken;
                    //Resmodal = await _iSSORepo.SSOTokenDetail(reqModel);
                    //Resmodal.sAMAccountName = "5466655522";
                    Resmodal.Status = 0;
                    if (Resmodal != null && Resmodal.Status == 1)
                    {

                        SSO_Response Responsemodel = new SSO_Response();
                        SSO_Request Requestmodel = new SSO_Request();
                        Requestmodel.SSO_ID = Resmodal.sAMAccountName;
                        // Requestmodel.SSO_ID = "JSHARMA.DOIT";
                        Responsemodel = await _iSSORepo.SSOLogin(Requestmodel);
                        Responsemodel.Token = SSOToken;

                        if (Responsemodel != null && Responsemodel.Status == 1)
                        {
                            MenuRequestModel menuMaster = new MenuRequestModel();

                            UserRequestModel reqLoginModel = new UserRequestModel();



                            #region claim bases authentication by Rajveer
                            //var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, Responsemodel.UserName, Responsemodel.RoleName);
                            // Adds the values to the claims we create.
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Id, Responsemodel.UserID.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Name, Responsemodel.UserName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Job, Responsemodel.RoleName));

                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.UserName, Responsemodel.RoleName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.UserId, Responsemodel.UserID.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.RoleName, Responsemodel.RoleName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.RoleId, Responsemodel.RoleId.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.RegistrationNo, Responsemodel.RegistrationNo));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Token, Responsemodel.Token));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LicenseeCode, Responsemodel.LicenseeCode));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LocationCode, Responsemodel.LocationCode.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Message, Responsemodel.Message));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Status, Responsemodel.Status.ToString()));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LocationName, Responsemodel.LocationName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.SSO_ID, Responsemodel.SSO_ID));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LocationType, Responsemodel.LocationType));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.FirstName, Responsemodel.FirstName));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Email, Responsemodel.Email));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.MobileNo, Responsemodel.MobileNo));
                            identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.UserRefID, Responsemodel.UserRefId.ToString()));



                            // Creates the context of user authentication.
                            var principal = new ClaimsPrincipal(identity);
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                                new AuthenticationProperties
                                {
                                    IsPersistent = true
                                });


                            #endregion


                            //UserResponseModel ResSSOmodel = new UserResponseModel();
                            //reqLoginModel.UserName = Responsemodel.UserName;
                            // ResSSOmodel = await _iSSORepo.LoginwithSSO(reqLoginModel);
                            //if (ResSSOmodel != null && ResSSOmodel.Status == 1)
                            // {
                            objSession = SetSession(Responsemodel);
                            objSession.IsSSOLogin = 1;
                            HttpContext.Session.SetComplexData("SessionDetails", objSession);

                            menuMaster.RoleId = Convert.ToInt32(Responsemodel.RoleId);
                            MenuResponseModel menuResponse = await _iMenu.MenuDetails(menuMaster);
                            HttpContext.Session.SetComplexData("MenuList", menuResponse.LstMenu);
                            HttpContext.Session.SetComplexData("RoleId", Convert.ToInt32(Responsemodel.RoleId));
                            HttpContext.Session.SetComplexData("UserId", Convert.ToInt32(Responsemodel.UserID));
                            HttpContext.Session.SetComplexData("UserRefId", Convert.ToInt32(Responsemodel.LicenseeCode));
                            HttpContext.Session.SetComplexData("LocationType", Responsemodel.LocationType.Trim());
                            HttpContext.Session.SetComplexData("LocationCode", Convert.ToInt32(Responsemodel.LocationCode));
                            HttpContext.Session.SetComplexData("MobileNumber", Responsemodel.MobileNo);
                            HttpContext.Session.SetComplexData("IsSSOLogin", objSession.IsSSOLogin);

                            //Session["MenuList"] = menuMaster.LstMenu;
                            //Session["UserName"] = Responsemodel.UserName;
                            //Session["UserId"] = Responsemodel.UserID;
                            //Session["RoleId"] = Responsemodel.RoleId;
                            //Session["RegistrationNo"] = Responsemodel.RegistrationNo;
                            //Session["IsSSOLogin"] = objSession.IsSSOLogin;

                            //Session["SSOToken"] = SSOToken;

                            ViewBag.token = "My Token is" + Responsemodel.Message;

                            if (Responsemodel.RoleId == 1)
                            {
                                return RedirectToAction("ImportBidders", "ImportBidders", new { Area = "RetailOff" });
                            }
                            else
                            {
                                return RedirectToAction("Dashboard", "Login");
                            }
                        }
                        else
                        {
                            return RedirectToAction("BackToSSO", "LoginConfermation");

                        }

                        //}
                    }
                    else
                    {
                        #region Get Citizen Role by Rajveer

                        return RedirectToAction("PublicServices", "LoginConfermation", new { token = SSOToken });
                        #endregion
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");

            }
            return View();
        }



        //[Route("PublicServices")]
        [HttpGet]
        public async Task<ActionResult> PublicServices(string token)
        {
            ListPublicServiceResponseModel model = new ListPublicServiceResponseModel();
            PublicServiceRequestModel requestModel = new PublicServiceRequestModel();
            model = await _iSSORepo.PublicService(requestModel);
            model.ssoName = await _iSSORepo.GetSSOIDFromToken(model.ssoModel.SSSOURL, token);
            model.token = token;
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PublicServices(PublicServiceResponseModel reqModel, string SSOID = "", string Dept = "Citizen")
        {
            #region Check IsSSO Exists
            int i = 0;
            string Url = "";
            string str = "";
            ListPublicServiceResponseModel model = new ListPublicServiceResponseModel();
            model = await _iSSORepo.GetTokenDetailJSON(reqModel);
            if (model != null && model.userDetails != null && !string.IsNullOrEmpty(model.userDetails.SSOID))
            {
                if (!string.IsNullOrEmpty(model.userDetails.SSOID))
                {
                    i = 1;
                    UserDetailSSOInformation m = new UserDetailSSOInformation();
                    m.aadhaarId = model.userDetails.aadhaarId;
                    m.firstName = model.userDetails.firstName;
                    m.lastName = model.userDetails.lastName;
                    m.postalAddress = model.userDetails.postalAddress;
                    m.postalCode = model.userDetails.postalCode;
                    m.SSOID = model.userDetails.SSOID;

                    str = Newtonsoft.Json.JsonConvert.SerializeObject(m);
                    Url = model.List.FirstOrDefault().ServiceUrl + "'" + str + "'";

                    if (Dept == "Dept")
                    {
                        PublicServiceDepartmentRequestModel obj = new PublicServiceDepartmentRequestModel();
                        obj = await _department.CheckUser(SSOID);
                        if (obj.Id > 0)
                        {
                            return RedirectToAction("SSOIndex", "LoginConfermation", new { Area = "", SSOID = SSOID });
                        }
                        else
                        {
                            TempData["Alert"] = CommonMessageServices.ShowAlert(Alerts.Warning, "User Does Not Exist");
                            i = 0;
                        }
                    }
                }
            }
            #endregion
            if (i > 0)
            {
                if (reqModel.ServiceUrl.Contains("http"))
                {
                    return Redirect(reqModel.ServiceUrl + reqModel.token);
                }
                else
                {

                    string[] link = reqModel.ServiceUrl.Split('/');
                    return RedirectToAction(link[2], link[1], new { area = link[0], SSODetails = _protector.Decode(str.ToString()) });
                }
            }
            else
                return RedirectToAction("PublicServices", "LoginConfermation", new { token = reqModel.token });
        }

        [HttpGet]
        public async Task<ActionResult> SSOIndex(string SSOID = "")
        {
            var SSOTokenExists = true;// Request.Form.TryGetValue("userdetails", out var SSOToken);

            if (SSOTokenExists)
            {
                try
                {
                    SSO_Response Responsemodel = new SSO_Response();
                    SSO_Request Requestmodel = new SSO_Request();
                    Requestmodel.SSO_ID = SSOID;
                    // Requestmodel.SSO_ID = "JSHARMA.DOIT";
                    Responsemodel = await _iSSORepo.SSOLogin(Requestmodel);

                    if (Responsemodel != null && Responsemodel.Status == 1)
                    {
                        MenuRequestModel menuMaster = new MenuRequestModel();

                        UserRequestModel reqLoginModel = new UserRequestModel();



                        #region claim bases authentication by Rajveer
                        //var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, Responsemodel.UserName, Responsemodel.RoleName);
                        // Adds the values to the claims we create.
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Id, Responsemodel.UserID.ToString()));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Name, Responsemodel.UserName));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Job, Responsemodel.RoleName));

                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.UserName, Responsemodel.UserName));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.UserId, Responsemodel.UserID.ToString()));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.RoleName, Responsemodel.RoleName));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.RoleId, Responsemodel.RoleId.ToString()));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.RegistrationNo, Responsemodel.RegistrationNo));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Token, Responsemodel.Token ?? ""));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LicenseeCode, Responsemodel.LicenseeCode));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LocationCode, Responsemodel.LocationCode.ToString()));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Message, Responsemodel.Message));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Status, Responsemodel.Status.ToString()));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LocationName, Responsemodel.LocationName));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.SSO_ID, Responsemodel.SSO_ID));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.LocationType, Responsemodel.LocationType));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.FirstName, Responsemodel.FirstName));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.Email, Responsemodel.Email));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.MobileNo, Responsemodel.MobileNo));
                        identity.AddClaim(new Claim(IdentityExtensions.CustomClaimTypes.UserRefID, Responsemodel.UserRefId.ToString()));



                        // Creates the context of user authentication.
                        var principal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = true
                            });


                        #endregion

                        objSession = SetSession(Responsemodel);
                        objSession.IsSSOLogin = 1;
                        HttpContext.Session.SetComplexData("SessionDetails", objSession);

                        menuMaster.RoleId = Convert.ToInt32(Responsemodel.RoleId);
                        MenuResponseModel menuResponse = await _iMenu.MenuDetails(menuMaster);
                        HttpContext.Session.SetComplexData("MenuList", menuResponse.LstMenu);
                        HttpContext.Session.SetComplexData("IsSSOLogin", objSession.IsSSOLogin);
                        HttpContext.Session.SetComplexData("LoginName", Convert.ToString(Responsemodel.FirstName));

                        if (Responsemodel.RoleId == 1)
                        {
                            return RedirectToAction("ImportBidders", "ImportBidders", new { Area = "RetailOff" });
                        }
                        else
                        {
                            return RedirectToAction("Dashboard", "Login");
                        }
                    }
                    else
                    {
                        return RedirectToAction("BackToSSO", "LoginConfermation");

                    }

                }
                catch (Exception)
                {

                }
            }
            else
            {
                return RedirectToAction("Login", "Login");

            }
            return View();
        }

        //[Route("LoginConfermation")]
        //[HttpPost]
        //public async Task<ActionResult> Index()
        //{
        //    var SSOTokenExists = Request.Form.TryGetValue("userdetails", out var SSOToken);
        //    if (SSOTokenExists)
        //    {
        //        // Session["SSOToken"] = SSOToken;
        //        SSO_API_Token reqModel = new SSO_API_Token();
        //        SSO_Token_Detail Resmodal = new SSO_Token_Detail();
        //        SessionDetails objSession = new SessionDetails();
        //        reqModel.API_Type = "/SSOREST/GetTokenDetailNewJson";
        //        reqModel.Token = SSOToken;
        //        Resmodal = await _iSSORepo.SSOTokenDetail(reqModel);
        //        ViewBag.SSOToken = SSOToken;
        //        ViewBag.UserID = Resmodal.sAMAccountName;


        //    }
        //    else 
        //    {
        //        ViewBag.SSOToken = "NO TOKEN";
        //    }
        //    return View();
        //}







        [HttpGet]
        public ActionResult BackToSSO()
        {
            SSO_Response model = new SSO_Response();
            try
            {
                model.Token = TempData.Get<string>("SSOToken");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult BacktoHome()
        {

            return View();
        }

        #region Set Session
        private SessionDetails SetSession(SSO_Response objLogin)
        {
            SessionDetails objSession = new SessionDetails();
            try
            {
                objSession.UserID = objLogin.UserID;
                objSession.FirstName = objLogin.FirstName;

                objSession.UserName = objLogin.UserName;
                objSession.RoleName = objLogin.RoleName;
                objSession.RoleId = objLogin.RoleId;
                objSession.RoleName = objLogin.RoleName;
                objSession.RegistrationNo = objLogin.RegistrationNo;
                objSession.Token = objLogin.Token;
                objSession.LicenseeCode = objLogin.LicenseeCode;
                objSession.LocationCode = objLogin.LocationCode;
                objSession.LocationType = objLogin.LocationType;
                objSession.Email = objLogin.Email;
                objSession.MobileNo = objLogin.MobileNo;
                objSession.LocationType = objLogin.LocationType;
                objSession.LocationName = objLogin.LocationName;
                objSession.SSO_ID = objLogin.SSO_ID;
                objSession.Token = objLogin.Token;
                objSession.LicenseeType = objLogin.LicenseeType;
                objSession.RoleCode = objLogin.RoleCode;
                HttpContext.Session.SetComplexData("RegistrationNo", Convert.ToInt32(objLogin.RegistrationNo));
                HttpContext.Session.SetComplexData("RoleName", Convert.ToString(objLogin.RoleName));
                HttpContext.Session.SetComplexData("LoginName", Convert.ToString(objLogin.FirstName));

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objSession;
        }
        #endregion


    }
}