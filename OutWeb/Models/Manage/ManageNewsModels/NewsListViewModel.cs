using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageNewsModels
{
    public class NewsListViewModel
    {
        NewsListFilterModel m_filter = new NewsListFilterModel();
        NewsListResultModel m_result = new NewsListResultModel();

        public NewsListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public NewsListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}