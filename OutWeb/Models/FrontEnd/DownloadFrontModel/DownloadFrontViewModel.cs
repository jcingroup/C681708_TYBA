using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.DownloadFrontModel
{
    public class DownloadFrontViewModel
    {
        DownloadFrontResultModel m_result = new DownloadFrontResultModel();
        public DownloadFrontResultModel Result { get { return m_result; } set { m_result = value; } }
        public int Page { get; set; }

    }
}