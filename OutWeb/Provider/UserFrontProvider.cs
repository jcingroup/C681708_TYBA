using OutWeb.Enums;
using OutWeb.Models.FrontEnd.UserInfo;
using OutWeb.Modules.FontEnd;
using System;
using System.Web;

namespace OutWeb.Provider
{
    public class UserFrontProvider
    {
        private UserFrontProvider()
        {
        }

        private static UserFrontProvider m_userSso = null;
        private LoginUserInfoModel m_user = null;

        public static UserFrontProvider Instance
        {
            get
            {
                if (m_userSso == null)
                    m_userSso = new UserFrontProvider();
                return m_userSso;
            }
        }

        private static HttpContext Context
        { get { return HttpContext.Current; } }

        public LoginUserInfoModel User
        {
            get
            {
                if (Context.Session["UserFrontInfo"] == null)
                    return null;
                if (!(Context.Session["UserFrontInfo"] is LoginUserInfoModel))
                    return null;
                return (LoginUserInfoModel)Context.Session["UserFrontInfo"];
            }
        }

        public void SignIn(LogInModel user)
        {
            LogInModule module = new LogInModule();
            m_user = module.GetUserBySignID(user);
            if (m_user == null)
                throw new Exception("請輸入正確帳號或密碼");
            if (m_user.UserAccount == "manager")
                m_user.Role = UserRoleEnum.SUPERADMIN;
            else if (m_user.UserAccount == "admin")
                m_user.Role = UserRoleEnum.ADMIN;
            m_user.Role = UserRoleEnum.USER;
            Context.Session["UserFrontInfo"] = m_user;
        }

        /// <summary>
        /// 使用者登出系統
        /// </summary>
        /// <returns></returns>
        public bool SignOut()
        {
            if (User != null)
            {
                Context.Session.Remove("UserFrontInfo");
            }
            return true;
        }
    }
}