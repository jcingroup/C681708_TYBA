using System.Collections.Generic;
using System.Web;

namespace OutWeb.Models.Manage.ActivityModels
{
    public class ActivityDataModel
    {
        private List<HttpPostedFileBase> m_files = new List<HttpPostedFileBase>();
        public List<HttpPostedFileBase> Files { get { return m_files; } set { m_files = value; } }

        private List<int> m_oldFilesId = new List<int>();
        public List<int> OldFilesId { get { return m_oldFilesId; } set { m_oldFilesId = value; } }

        public int? ID { get; set; }

        public string Title { get; set; }
        public double Sort { get; set; }

        public string PublishDateStr { get; set; }
        public string contenttext { get; set; }
        public string ActivityNumber { get; set; }

        public bool Disable { get; set; }

        public string ActivityDateTimeDescription { get; set; }

        public string ApplyDateTimeBegin { get; set; }
        public string ApplyDateTimeEnd { get; set; }
        private List<ActivityGroup> m_activityGroup = new List<ActivityGroup>();
        public List<ActivityGroup> ActivityGroup { get { return m_activityGroup; } set { m_activityGroup = value; } }
    }
}

public class ActivityGroup
{
    public int GroupID { get; set; }
    public string GroupName { get; set; }
    public int GroupApplyLimit { get; set; }
    public int CountApplyLimit { get; set; }

}