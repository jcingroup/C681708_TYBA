using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.IPModels
{
    public class IPListViewModel
    {
        IPListFilterModel m_filter = new IPListFilterModel();
        IPListResultModel m_result = new IPListResultModel();

        public IPListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public IPListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}