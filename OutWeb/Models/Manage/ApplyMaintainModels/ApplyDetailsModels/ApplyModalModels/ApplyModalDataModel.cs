using OutWeb.Models.FrontEnd.ApplyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyModalModels
{
    [Serializable]
    public class ApplyModalDataModel
    {
        public bool ApplyStatus { get; set; }
        public int ActivityID { get; set; }
        public string ActivityTitle { get; set; }
        public string TeamName { get; set; }
        public int ApplyID { get; set; }
        public int ApplyGroupID { get; set; }
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

        public string ApplyGroupJsonString { get; set; }
        private List<ApplyViewGroup> m_activityGroup = new List<ApplyViewGroup>();
        public List<ApplyViewGroup> ActivityGroup { get { return m_activityGroup; } set { m_activityGroup = value; } }

    }
    [Serializable]
    public class MemberInfo
    {
        public int? ID { get; set; }
        public string MemberName { get; set; }
        public string MemberIdentityID { get; set; }
        public string MemberBirthday { get; set; }
        /// <summary>
        /// 成員類別  隊長：Leader  隊員：Member
        /// </summary>
        public string MemberType { get; set; }
    }

}