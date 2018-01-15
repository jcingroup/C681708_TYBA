using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageNewsModels
{
    public class NotificationListViewModel
    {
        NotificationListFilterModel m_filter = new NotificationListFilterModel();
        NotificationListResultModel m_result = new NotificationListResultModel();

        public NotificationListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public NotificationListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}