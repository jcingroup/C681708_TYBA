
using OutWeb.Entities;
using OutWeb.Models.FrontEnd.UserInfo;
using OutWeb.Provider;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Modules.FontEnd
{

    public class LogInModule:IDisposable
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public void Dispose()
        {
            if (this.DB.Database.Connection.State == System.Data.ConnectionState.Open)
            {
                this.DB.Database.Connection.Close();
            }
            this.DB.Dispose();
            this.DB = null;
        }



        /// <summary>
        /// 取得使用者資訊
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public LoginUserInfoModel GetUserBySignID(LogInModel userModel)
        {
            LoginUserInfoModel userInfo =
            this.DB.TempValidUser
                .Where(s => s.Account == userModel.Account && s.Password == userModel.Password)
                         .Select(s => new LoginUserInfoModel()
                         {
                             ID = s.ID,
                             UserName = s.UserName,
                             UserAccount = s.Account
                         })
                         .FirstOrDefault();
            PublicMethodRepository.HtmlDecode(userInfo);
            return userInfo;
        }


        //public bool ChangePassword(FormCollection form)
        //{
        //    if (UserFrontProvider.Instance.User == null)
        //    {
        //        throw new Exception("請先登入!");
        //    }
        //    var oldPwd = form["oldPw"];
        //    var newPwd = form["newPw"];
        //    var rePwd = form["rePw"];
        //    var entityUser = this.DB.WBUSR.Where(o => o.SIGNIN_ID == UserFrontProvider.Instance.User.UserAccount).First();
        //    bool isTruePw = (oldPwd + UserFrontProvider.Instance.User.GUID == entityUser.SIGNIN_PWD + UserFrontProvider.Instance.User.GUID);
        //    if (isTruePw)
        //    {
        //        if (newPwd.Equals(rePwd))
        //        {
        //            entityUser.SIGNIN_PWD = rePwd;
        //            this.DB.Entry(entityUser).State = EntityState.Modified;
        //            this.DB.SaveChanges();
        //        }
        //        else
        //            throw new Exception("新密碼兩次輸入密碼不同.");
        //    }
        //    else
        //        throw new Exception("原密碼輸入錯誤.");
        //    return true;
        //}
    }
}