
using OutWeb.Entities;
using OutWeb.Models.UserInfo;
using OutWeb.Provider;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Modules.Manage
{

    public class SignInModule : IDisposable
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }


        /// <summary>
        /// 取得使用者資訊
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public LoginUserInfoModel GetUserBySignID(SignInModel userModel)
        {
            LoginUserInfoModel userInfo =
            this.DB.人員
                .Where(s => s.帳號 == userModel.Account && s.密碼 == userModel.Password)
                         .Select(s => new LoginUserInfoModel()
                         {
                             ID = s.人員代碼,
                             UserAccount = s.帳號,
                             UserName = s.姓名,
                             IsDisabled = s.停權,
                             UnitCode = (int)s.單位代碼,
                         })
                         .FirstOrDefault();
            PublicMethodRepository.HtmlDecode(userInfo);
            return userInfo;
        }


        public bool ChangePassword(FormCollection form)
        {
            if (UserProvider.Instance.User == null)
            {
                throw new Exception("請先登入!");
            }
            var oldPwd = form["oldPw"];
            var newPwd = form["newPw"];
            var rePwd = form["rePw"];
            var entityUser = this.DB.人員.Where(o => o.人員代碼 == UserProvider.Instance.User.ID).First();
            PublicMethodRepository.HtmlDecode(entityUser);
            foreach (var f in form.AllKeys)
                PublicMethodRepository.FilterXss(form[f]);
            bool isTruePw = (oldPwd == entityUser.密碼);
            if (isTruePw)
            {
                if (newPwd.Equals(rePwd))
                {
                    entityUser.密碼 = rePwd;
                    this.DB.Entry(entityUser).State = EntityState.Modified;
                    this.DB.SaveChanges();
                }
                else
                    throw new Exception("新密碼兩次輸入密碼不同.");
            }
            else
                throw new Exception("原密碼輸入錯誤.");
            return true;
        }

        public void Dispose()
        {
            if (this.DB.Database.Connection.State == System.Data.ConnectionState.Open)
            {
                this.DB.Database.Connection.Close();
            }
            this.DB.Dispose();
            this.DB = null;
        }


    }
}