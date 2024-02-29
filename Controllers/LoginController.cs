using DNTCaptcha.Core;
using DocumentFormat.OpenXml.EMMA;
using IEMS_FrontApplications.Comman;
using IEMS_WEB.Areas.Wallet.Models.Response;
using IEMS_WEB.Comman;
using IEMS_WEB.Interface;
using IEMS_WEB.Models;
using IEMS_WEB.Models.RequestModel;
using IEMS_WEB.Models.ResponseModel;
using iTextSharp.text.pdf.qrcode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nancy.Session;
using System.Reflection;
using System.Security.Claims;

namespace IEMS_WEB.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogin _iLoginRepo;
        private readonly IMenu _iMenu;
        private readonly IDNTCaptchaValidatorService _captchaService;
        public LoginController(ILogin iLoginRepo, IMenu iMenu, IDNTCaptchaValidatorService captchaService)
        {
            _iLoginRepo = iLoginRepo;
            _iMenu = iMenu;
            _captchaService = captchaService;
        }
        // GET: Login
        [HttpGet]

        public ActionResult Login()
        {
            UserRequestModel reqModel = new UserRequestModel();
            return View(reqModel);
        }
        /// <summary>
        /// Home landing Page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult HomeLanding()
        {

            return View();
        }
        [HttpGet]
        public ActionResult Dashboard()
        {

            return View();
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync();
            // Remove cookies and redirect to login
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            #region Get User Login Information by identity rajveer
            // int UserID = User.Identity.GetId();
            #endregion
            return RedirectToAction("Login");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[CustomExceptionHandlerFilter]
        public async Task<ActionResult> Login(UserRequestModel reqModel)
        {
            //int i = 0;
            //int b= 10/i;
            UserResponseModel model = new UserResponseModel();
            SessionDetails objSession = new SessionDetails();
            if (reqModel.Password != null)
            {
                reqModel.Password = GeneralFunctions.DecryptStringAES(reqModel.Password);
            }
            if (reqModel.UserName != null)
            {
                reqModel.UserName = GeneralFunctions.DecryptStringAES(reqModel.UserName);
            }
            //if (this.IsCaptchaValid("Captcha is not valid"))
            //{
            //if (!_captchaService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
            //{
            //    // this.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please Enter Valid Captcha.");
            //    ViewBag.ErrMessage = "Please enter valid captcha";
            //    return View("Login");
            //}
            try
            {
                model = await _iLoginRepo.Login(reqModel);
                if (model != null && model.Status == 1)
                {
                    #region Set Token
                    model.Token = model.model.FirstOrDefault().Token;
                    HttpContext.Session.SetString("token", model.Token);
                    #endregion
                    #region Set Session
                    objSession = SetSession(model);
                    HttpContext.Session.SetComplexData("SessionDetails", objSession);
                    #endregion
                    #region claim bases authentication by Rajveer
                    //var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, model.UserName, model.RoleName);
                    // Adds the values to the claims we create.
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Id, model.UserID.ToString()));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Name, model.UserName));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Job, model.RoleName));

                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.UserName, model.RoleName));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.UserId, model.UserID.ToString()));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.RoleName, model.RoleName));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.RoleId, model.RoleId.ToString()));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.RegistrationNo, model.RegistrationNo));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Token, model.Token));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.LicenseeCode, model.LicenseeCode));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.LocationCode, model.LocationCode.ToString()));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Message, model.Message));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Status, model.Status.ToString()));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.LocationName, model.LocationName));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.SSO_ID, model.SSO_ID));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.LocationType, model.LocationType));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.FirstName, model.FirstName));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Email, model.Email));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.MobileNo, model.MobileNo));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.UserRefID, model.UserRefId.ToString()));
                    identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.LicenseeType, model.LicenseeType));


                    // Creates the context of user authentication.
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true
                        });


                    #endregion

                    MenuRequestModel menuRequestModel = new MenuRequestModel();
                    menuRequestModel.RoleId = Convert.ToInt32(model.RoleId);//model.model.FirstOrDefault().RoleId ?? 0;
                    HttpContext.Session.SetComplexData("RoleId", Convert.ToInt32(model.RoleId));
                    HttpContext.Session.SetComplexData("UserId", Convert.ToInt32(model.UserID));
                    HttpContext.Session.SetComplexData("UserRefId", Convert.ToInt32(model.UserRefId));
                    HttpContext.Session.SetComplexData("LocationType", model.LocationType.Trim());
                    HttpContext.Session.SetComplexData("LocationCode", Convert.ToInt32(model.LocationCode));
                    HttpContext.Session.SetComplexData("MobileNumber", model.MobileNo);
                    HttpContext.Session.SetComplexData("LicenseeCode", Convert.ToInt32(model.LicenseeCode));
                    HttpContext.Session.SetComplexData("IsSSOLogin", 0);


                    #region Get Menu List
                    MenuResponseModel respModel = await _iMenu.MenuDetails(menuRequestModel);
                    #endregion
                    if (respModel != null && respModel.LstMenu.Count > 0)
                    {
                        HttpContext.Session.SetComplexData("MenuList", respModel.LstMenu);
                        //var d = HttpContext.Session.GetComplexData<List<MenuMaster>>("MenuList");

                        if (model.RoleId == 1)
                        {
                            return RedirectToAction("ImportBidders", "ImportBidders", new { Area = "RetailOff" });

                        }
                        else
                        {
                            return RedirectToAction("Dashboard", "Login");

                        }


                    }
                    return RedirectToAction("Login", "Login");
                }
                else if (model != null && (model.Status == -2 || model.Status == -3))
                {
                    HttpContext.Session.SetComplexData("UserId", Convert.ToInt32(model.UserID));
                    HttpContext.Session.SetComplexData("UserName", Convert.ToString(reqModel.UserName));
                    HttpContext.Session.SetComplexData("Password", Convert.ToString(reqModel.Password));
                    HttpContext.Session.Remove("SessionDetails");
                    return RedirectToAction("ChangePassword", "Login");

                }
                else
                {
                    ViewBag.ErrMessage = model.Message;
                    ViewBag.ErrCode = model.Status;

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            //}
            //else
            //{
            //    ViewBag.ErrMessage = "Invalid Captcha";
            //}
            return View(reqModel);
        }
        public async Task<ActionResult> GetDetailsForOTPLogin(string UserName)
        {
            UserResponseModel resModel = new UserResponseModel();
            SessionDetails objSession = new SessionDetails();
            MenuMaster menuMaster = new MenuMaster();
            try
            {
                resModel = await _iLoginRepo.DetailsForOTPLogin(UserName);
                if (resModel != null && resModel.Status == 1)
                {
                    //SessionIDManager SessionManager = new SessionIDManager();
                    //string SessionId = SessionManager.CreateSessionID(System.Web.HttpContext.Current);
                    #region Set Token
                    resModel.Token = resModel.model.FirstOrDefault().Token;
                    HttpContext.Session.SetString("token", resModel.Token);
                    #endregion
                    #region Set Session
                    objSession = SetSession(resModel);
                    HttpContext.Session.SetComplexData("SessionDetails", objSession);
                    #endregion

                    HttpContext.Session.SetComplexData("RoleId", Convert.ToInt32(resModel.RoleId));
                    HttpContext.Session.SetComplexData("UserId", Convert.ToInt32(resModel.UserID));
                    HttpContext.Session.SetComplexData("UserRefId", Convert.ToInt32(resModel.UserRefId));
                    HttpContext.Session.SetComplexData("LocationType", resModel.LocationType.Trim());
                    HttpContext.Session.SetComplexData("LocationCode", Convert.ToInt32(resModel.LocationCode));
                    HttpContext.Session.SetComplexData("MobileNumber", resModel.MobileNo);
                    HttpContext.Session.SetComplexData("LicenseeCode", Convert.ToInt32(resModel.LicenseeCode));
                    HttpContext.Session.SetComplexData("IsSSOLogin", 0);

                    MenuRequestModel menuRequestModel = new MenuRequestModel();
                    menuRequestModel.RoleId = Convert.ToInt32(resModel.RoleId);//model.model.FirstOrDefault().RoleId ?? 0;
                    #region Get Menu List
                    MenuResponseModel respModel = await _iMenu.MenuDetails(menuRequestModel);
                    #endregion
                    if (respModel != null && respModel.LstMenu.Count > 0)
                    {
                        HttpContext.Session.SetComplexData("MenuList", respModel.LstMenu);
                        //var d = HttpContext.Session.GetComplexData<List<MenuMaster>>("MenuList");
                        #region claim bases authentication by Rajveer
                        //var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, resModel.UserName, resModel.RoleName);
                        // Adds the values to the claims we create.
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Id, resModel.UserID.ToString()));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Name, resModel.UserName));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Job, resModel.RoleName));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.UserName, resModel.RoleName));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.UserId, resModel.UserID.ToString()));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.RoleName, resModel.RoleName));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.RoleId, resModel.RoleId.ToString()));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.RegistrationNo, resModel.RegistrationNo));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Token, resModel.Token));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.LicenseeCode, resModel.LicenseeCode));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.LocationCode, resModel.LocationCode.ToString()));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Message, resModel.Message));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Status, resModel.Status.ToString()));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.LocationName, resModel.LocationName));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.SSO_ID, resModel.SSO_ID));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.LocationType, resModel.LocationType));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.FirstName, resModel.FirstName));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.Email, resModel.Email));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.MobileNo, resModel.MobileNo));
                        identity.AddClaim(new Claim(Comman.IdentityExtensions.CustomClaimTypes.UserRefID, resModel.UserRefId.ToString()));



                        // Creates the context of user authentication.
                        var principal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = true
                            });


                        #endregion




                        return Json(resModel);
                        //if (resModel.RoleId == 1)
                        //{
                        //   return Json(resModel);
                        //    return RedirectToAction("ImportBidders", "ImportBidders", new { Area = "RetailOff" });

                        //}
                        //else
                        //{
                        //    return RedirectToAction("Dashboard", "Login");

                        //}
                    }
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.ErrMessage = resModel.Message;
                }
            }
            catch (Exception ex)
            {
                resModel.Status = -1;
            }
            return RedirectToAction("Login", "Login");
        }
        [HttpGet]
        public ActionResult Refersh()
        {
            return PartialView("Captcha");
        }
        private SessionDetails SetSession(UserResponseModel objLogin)
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
                objSession.LicenseeType = objLogin.LicenseeType;
                objSession.RoleCode = objLogin.RoleCode;
                ////TEMPORARY
                //Session["UserName"] = objLogin.UserName;
                //Session["UserId"] = objLogin.UserID;
                //Session["RoleId"] = objLogin.RoleId;
                //Session["RegistrationNo"] = objLogin.RegistrationNo;
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


        #region Forgot Password
        public async Task<ActionResult> GenerateOtp()
        {
            ValidateUserName model = new ValidateUserName();
             string UserName = Request.Form["UserName"];
            string DecryptedUsername = string.Empty;
            try
            {
                if (UserName != null || UserName !="")
                {
                    DecryptedUsername = GeneralFunctions.DecryptStringAES(UserName);
                }
                model = await _iLoginRepo.ValidateUserName(DecryptedUsername);
                if (model.Status == 1)
                {
                    string ResponseMessage = await _iLoginRepo.GenerateTotp(Convert.ToString(model.MobileNo));
                   // model.OTP = ResponseMessage;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return Json(model);
        }
        public async Task<ActionResult> SaveForgetPasword()
        {
            ForgetPassword reqModel = new ForgetPassword();
            return View(reqModel);
        }
        [HttpPost]
        public async Task<ActionResult> SaveForgetPasword(ForgetPassword reqModel)
        {
            BaseModel model = new BaseModel();
            ForgetPassword resModel = new ForgetPassword();

            model = await _iLoginRepo.SaveForgetPasword(reqModel);
            resModel.Status = model.Status;
            resModel.StatusMessage = model.Message;
            return View(resModel);
        }

        [HttpGet]
        public async Task<ActionResult> ViewOTPPartialView(string MobileNumber, string FormKey)
        {
            OTPViewModel OTPPartialModel = new OTPViewModel();
            OTPPartialModel.OTPObj.MobileNumber = MobileNumber;
            OTPPartialModel.OTPObj.OTPKey = FormKey;
            string ResponseMessage = await _iLoginRepo.GenerateTotp(Convert.ToString(MobileNumber));
            OTPPartialModel.ResponseMessage = ResponseMessage;
            return PartialView("_OTP", OTPPartialModel);
        }
        [HttpPost]
        public async Task<ActionResult> ValidateOTP()
        {
            OTPResponse model = new OTPResponse();
            OTP objReq= new OTP();
            string EncryOtp = Request.Form["OTPVal"];
            string EncryMobNo = Request.Form["MobileNumber"];
            if (EncryOtp != null || EncryOtp != "")
            {
                objReq.OTPVal = GeneralFunctions.DecryptStringAES(EncryOtp);
                objReq.MobileNumber = GeneralFunctions.DecryptStringAES(EncryMobNo);
            }
            model = await _iLoginRepo.ValidateTotp(objReq);
            return Json(model);
        }
        #endregion

        #region Change Password
        public ActionResult ChangePassword()
        {
            ChangePassowrd reqModel = new ChangePassowrd();
            SessionDetails model = new SessionDetails();
            model = HttpContext.Session.GetComplexData<SessionDetails>("SessionDetails");
            reqModel.CurrentPassword = HttpContext.Session.GetComplexData<string>("Password");
            if (model != null)
            {
                reqModel.UserId = model.UserID;
                HttpContext.Session.SetComplexData("UserId", Convert.ToInt32(model.UserID));
                HttpContext.Session.SetComplexData("UserName", Convert.ToString(model.UserName));
            }
            else
            {
                reqModel.UserId = 0;
            }

            return View(reqModel);
        }
        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePassowrd reqModel)
        {
            BaseModel model = new BaseModel();
            ChangePassowrd resModel = new ChangePassowrd();
            reqModel.UserId = HttpContext.Session.GetComplexData<Int32>("UserId");
            reqModel.UserName = HttpContext.Session.GetComplexData<string>("UserName");
            HttpContext.Session.SetComplexData("Password", Convert.ToString(reqModel.NewPassword));
            model = await _iLoginRepo.SaveChangePassword(reqModel);
            resModel.status = model.Status;
            resModel.StatusMessage = model.Message;

            return View(resModel);
        }
        #endregion


    }

}
