
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
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
        { get { return this.m_DB; } set { this.m_DB = value; } }


        /// <summary>
        /// 取得使用者資訊
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public LoginUserInfoModel GetUserBySignID(SignInModel userModel)
        {
            LoginUserInfoModel userInfo =
            this.DB.USER
                .Where(s => s.USR_ID == userModel.Account && s.USR_PWD == userModel.Password)
                         .Select(s => new LoginUserInfoModel()
                         {
                             ID = s.ID,
                             UserAccount = s.USR_ID,
                             UserName = s.USR_NM,
                             IsDisabled = s.DISABLE,
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
            var entityUser = this.DB.USER
                .Where(o => o.ID == UserProvider.Instance.User.ID &&
            o.USR_ID == UserProvider.Instance.User.UserAccount).First();
            PublicMethodRepository.HtmlDecode(entityUser);


            bool isTruePw = (oldPwd == entityUser.USR_PWD);
            if (isTruePw)
            {
                if (newPwd.Equals(rePwd))
                {
                    PublicMethodRepository.FilterXss(entityUser);
                    entityUser.USR_PWD = rePwd;
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