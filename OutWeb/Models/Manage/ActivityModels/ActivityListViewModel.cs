using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ActivityModels
{
    public class ActivityListViewModel
    {
        ActivityFilterModel m_filter = new ActivityFilterModel();
        ActivityListResultModel m_result = new ActivityListResultModel();

        public ActivityFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public ActivityListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}