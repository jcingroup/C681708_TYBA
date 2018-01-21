using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.BannerModels
{
    public class BannerDataModel
    {
        private List<HttpPostedFileBase> m_files = new List<HttpPostedFileBase>();
        public List<HttpPostedFileBase> Files { get { return m_files; } set { m_files = value; } }

        private List<int> m_oldFilesId = new List<int>();
        public List<int> OldFilesId { get { return m_oldFilesId; } set { m_oldFilesId = value; } }

        public int? ID { get; set; }

        public string Title { get; set; }
        public double Sort { get; set; }

        public string PublishDateStr { get; set; }


        public bool Disable { get; set; }
    }
}