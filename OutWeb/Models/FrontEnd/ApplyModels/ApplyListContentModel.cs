using OutWeb.Models.FrontEnd.ApplyModels;
using OutWeb.Models.Manage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.DownloadFrontModel
{
    public class ApplyListContentModel
    {
        List<FileViewModel> m_files = new List<FileViewModel>();
        public List<FileViewModel> Files { get { return m_files; } set { m_files = value; } }

        ApplyListViewDataModel m_data = new ApplyListViewDataModel();
        public ApplyListViewDataModel Data { get { return m_data; } set { m_data = value; } }
    }
}