using OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyDetailsListModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels
{
    public class ApplyDetailsDataModel
    {
        public int ActivityID { get; set; }
        public string ActivityDateRange { get; set; }
        public string ActivityName { get; set; }

        private Dictionary<int, string> m_groupList = new Dictionary<int, string>();
        public Dictionary<int, string> GroupList { get { return m_groupList; } set { m_groupList = value; } }


        private ApplyDetailsListViewModel m_list = new ApplyDetailsListViewModel();
        public ApplyDetailsListViewModel ListData { get { return m_list; } set { m_list = value; } }
        private GroupModel m_gorup = new GroupModel();
        public GroupModel GroupInfo { get { return m_gorup; } set { m_gorup = value; } }
    }
    public class GroupModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int GroupApplyLimit { get; set; }
        public int MemberApplyLimit { get; set; }
        /// <summary>
        /// 已報名隊伍 不分組別
        /// </summary>
        public int RegisteredCount { get; set; }

        /// <summary>
        /// 已報名完成隊伍 不分組別
        /// </summary>
        public int RegisteredSuccessCount { get; set; }
    }
}