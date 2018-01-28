using System;
using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.ApplyModels
{
    [Serializable]
    public class ApplyDataModel
    {
        public int ActivityID { get; set; }
        public string ActivityTitle { get; set; }
        public string TeamName { get; set; }
        public int ApplyGroupID { get; set; }
        public string ApplyGroupName { get; set; }
        public string Contact { get; set; }
        public string ContactPhone { get; set; }
        /// <summary>
        /// 教練
        /// </summary>
        public string Coach { get; set; }

        public string Email { get; set; }
        public string Remark { get; set; }
        public string ApplyNumber { get; set; }

        private List<MemberInfo> m_member = new List<MemberInfo>();
        public List<MemberInfo> Member { get { return m_member; } set { m_member = value; } }

    }
    [Serializable]
    public class MemberInfo
    {
        public string MemberName { get; set; }
        public string MemberIdentityID { get; set; }
        public string MemberBirthday { get; set; }
        /// <summary>
        /// 成員類別  隊長：Leader  隊員：Member
        /// </summary>
        public string MemberType { get; set; }
    }
}