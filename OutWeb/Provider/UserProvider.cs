using OutWeb.Enums;
using OutWeb.Models.UserInfo;
using OutWeb.Modules.Manage;
using System;
using System.Web;

namespace OutWeb.Provider
{
    public class UserProvider
    {
        private UserProvider()
        {
        }

        private static UserProvider m_userSso = null;
        private LoginUserInfoModel m_user = null;

        public static UserProvider Instance
        {
            get
            {
                if (m_userSso == null)
                    m_userSso = new UserProvider();
                return m_userSso;
            }
        }

        private static HttpContext Context
        { get { return HttpContext.Current; } }

        public LoginUserInfoModel User
        {
            get
            {
                if (Context.Session["UserInfo"] == null)
                    return null;
                if (!(Context.Session["UserInfo"] is LoginUserInfoModel))
                    return null;
                return (LoginUserInfoModel)Context.Session["UserInfo"];
            }
        }

        public void SignIn(SignInModel user)
        {
            SignInModule module = new SignInModule();
            m_user = module.GetUserBySignID(user);
            if (m_user == null)
                throw new Exception("請輸入正確帳號或密碼");
            if (m_user.UserAccount == "manager")
                m_user.Role = UserRoleEnum.SUPERADMIN;
            else if (m_user.UserAccount == "admin")
                m_user.Role = UserRoleEnum.ADMIN;
            else
                m_user.Role = UserRoleEnum.USER;
            Context.Session["UserInfo"] = m_user;
            //寫入登入紀錄
            string ip = System.Web.HttpContext.Current.Request.UserHostAddress;
            IpHistoryModule ipMdu = new IpHistoryModule();
            ipMdu.WriteIp(ip, "manager");
            ipMdu.Dispose();
        }

        /// <summary>
        /// 使用者登出系統
        /// </summary>
        /// <returns></returns>
        public bool SignOut()
        {
            if (User != null)
            {
                Context.Session.Remove("UserInfo");
            }
            return true;
        }
    }
}