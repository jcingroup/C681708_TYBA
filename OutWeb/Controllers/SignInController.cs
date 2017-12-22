using BotDetect.Web.Mvc;
using OutWeb.Models.UserInfo;
using OutWeb.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace OutWeb.Controllers
{
    public class SignInController : Controller
    {


        /// <summary>
        /// 登入頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("_SysAdm")]
        [Route("_SysAdm/Index")]
        public ActionResult Login()
        {
            try
            {
                if (UserProvider.Instance.User == null)
                    return View();
                else
                    return RedirectToAction("NewsList", "_SysAdm", null);
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
        [Route("_SysAdm/Index")]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("CaptchaCode", "ExampleCaptcha", "驗證碼輸入錯誤!")]
        public ActionResult Login(SignInModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UserProvider.Instance.SignIn(model);
                }
                catch (System.Exception ex)
                {

                    return RedirectToAction("SignInFail", new { exMessage = ex.Message });
                }
            }
            else
                return View();

            return RedirectToAction("NewsList", "_SysAdm");
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [Route("~/_SysAdm/LogOut")]
        public ActionResult LogOut()
        {
            UserProvider.Instance.SignOut();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// 登入失敗
        /// </summary>
        /// <param name="exMessage"></param>
        /// <returns></returns>
        public ActionResult SignInFail(string exMessage)
        {
            if (exMessage != null)
                TempData["SignInFail"] = exMessage;
            else
                TempData["SignInFail"] = "尚未登入，未持有管理權限.";
            return RedirectToAction("Login");
        }
    }
}