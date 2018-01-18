using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.DownloadModels
{
    public class DownLoadListViewModel
    {
        DownloadFilterModel m_filter = new DownloadFilterModel();
        DownloadListResultModel m_result = new DownloadListResultModel();

        public DownloadFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public DownloadListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}