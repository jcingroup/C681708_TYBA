using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ResultModels
{
    public class ResultListViewModel
    {
        ResultListFilterModel m_filter = new ResultListFilterModel();
        ResultListResultModel m_result = new ResultListResultModel();

        public ResultListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public ResultListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}