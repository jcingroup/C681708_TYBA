using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.NewsFrontEndModels
{
    public class NewsListFrontViewModel 
    {
        private NewsListFrontFilterModel m_filter = new NewsListFrontFilterModel();
        public NewsListFrontFilterModel Filter { get { return m_filter; } set { m_filter = value; } }
        private NewsListFrontResultModel m_result = new NewsListFrontResultModel();
        public NewsListFrontResultModel Result { get { return m_result; } set { m_result = value; } }
   
    }
}