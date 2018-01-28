using OutWeb.Models.Manage;
using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.ApplyModels
{
    public class ApplyViewDetailsModel
    {
        private List<FileViewModel> m_files = new List<FileViewModel>();
        public List<FileViewModel> Files { get { return m_files; } set { m_files = value; } }

        public int? ID { get; set; }

        public string Title { get; set; }
        public double? Sort { get; set; }

        public string PublishDateStr { get; set; }
        public string ActivityContent { get; set; }
        public string ActivityNumber { get; set; }

        public bool Disable { get; set; }

        public string ActivityDateTimeDescription { get; set; }

        public string ApplyDateTimeBegin { get; set; }
        public string ApplyDateTimeEnd { get; set; }
        public string ApplyGroupJsonString { get; set; }
        private List<ApplyViewGroup> m_activityGroup = new List<ApplyViewGroup>();
        public List<ApplyViewGroup> ActivityGroup { get { return m_activityGroup; } set { m_activityGroup = value; } }
    }

    public class ApplyViewGroup
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int GroupApplyLimit { get; set; }
        public int CountApplyLimit { get; set; }

        /// <summary>
        /// 剩餘報名組數
        /// </summary>
        public int CountApplyLastLimit { get; set; }
    }
}