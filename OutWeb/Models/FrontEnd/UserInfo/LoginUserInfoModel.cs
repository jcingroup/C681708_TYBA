using OutWeb.Enums;
using System;

namespace OutWeb.Models.FrontEnd.UserInfo
{
    [Serializable]
    public class LoginUserInfoModel
    {
        /// <summary>
        /// 索引值
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 使用者帳號
        /// </summary>
        public string UserAccount{ get; set; }

        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 使用者英文名稱
        /// </summary>
        public string UserEngName { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 停權
        /// </summary>
       public bool IsDisabled { get; set; }

        /// <summary>
        /// GUID
        /// </summary>
        public DateTime CreateDate { get; set; }

        public string GUID { get; internal set; }

        /// <summary>
        /// 角色
        /// </summary>
        public UserRoleEnum Role { get; set; }

        /// <summary>
        /// 單位代碼
        /// </summary>
        public int UnitCode { get; set; }
    }
}