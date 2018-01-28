using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ActivityModels
{
    public class ActivityDetailsModel
    {
        List<FileViewModel> m_files = new List<FileViewModel>();
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
        private List<ActivityGroup> m_activityGroup = new List<ActivityGroup>();
        public List<ActivityGroup> ActivityGroup { get { return m_activityGroup; } set { m_activityGroup = value; } }
    }
}