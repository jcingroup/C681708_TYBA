using OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyDetailsListModels;
using OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyModalModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ApplyMaintainModels
{
    public class ApplyExcelReplyDataModel : ExcelReplyBase
    {
        public int ActivityID { get; set; }
        public string ActivityName { get; set; }

        private Dictionary<int, string> m_groupList = new Dictionary<int, string>();
        public Dictionary<int, string> GroupList { get { return m_groupList; } set { m_groupList = value; } }

        private List<ApplyListDetailsData> m_list = new List<ApplyListDetailsData>();
        public List<ApplyListDetailsData> ApplyListData { get { return m_list; } set { m_list = value; } }

    }

    public class ApplyListDetailsData
    {
        public int ID { get; set; }
        public string ApplyDate { get; set; }
        public string ApplyNumber { get; set; }
        public string ApplyTeamName { get; set; }
        public string Coach { get; set; }
        public string Contact { get; set; }
        public string Remark { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail{ get; set; }
        public int ApplyTeamMemberCount { get; set; }

        public bool ApplySuccessStatus { get; set; }
        public int GroupID { get; set; }
        private List<MemberInfo> m_member = new List<MemberInfo>();
        public List<MemberInfo> MemberInfo { get { return m_member; } set { m_member = value; } }
    }
}