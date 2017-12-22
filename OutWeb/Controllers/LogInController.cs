using OutWeb.ActionFilter;
using OutWeb.Models.FrontEnd.UserInfo;
using OutWeb.Provider;
using System;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
    public class LogInController : Controller
    {

        public LogInController()
        {
            ViewBag.IsFirstPage = false;
        }

        /// <summary>
        /// 登入頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Route("_SysAdm")]
        [Route("Home/Login")]
        public ActionResult Login(string redirectUrl)
        {
            ViewBag.RedirectUrl = redirectUrl;
            try
            {
                if (UserFrontProvider.Instance.User == null)
                    return View();
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 登入頁面 POST
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Home/Login")]
        [ValidateAntiForgeryToken]
        //[CaptchaValidation("CaptchaCode", "ExampleCaptcha", "驗證碼輸入錯誤!")]
        public ActionResult Login(LogInModel model, string redirectUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UserFrontProvider.Instance.SignIn(model);
                    if (!string.IsNullOrEmpty(redirectUrl))
                        return Redirect(redirectUrl);
                }
                catch
                {
                    TempData["SignInFail"] = "請輸入正確的帳號密碼.";
                    return View();
                }
            }
            else
                return View();

            return RedirectToAction("index", "Home");
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [Route("~/Home/LogOut")]
        public ActionResult LogOut()
        {
            UserFrontProvider.Instance.SignOut();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// 登入失敗
        /// </summary>
        /// <param name="exMessage"></param>
        /// <returns></returns>
        public ActionResult SignInFail(string exMessage, string redirectUrl)
        {
            if (exMessage != null)
                TempData["SignInFail"] = exMessage;
            else
                TempData["SignInFail"] = "尚未登入，請先登入.";
            return RedirectToAction("Login", new { redirectUrl = redirectUrl });
        }
    }
}