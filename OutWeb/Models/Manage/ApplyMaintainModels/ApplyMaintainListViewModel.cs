using OutWeb.Models.Manage.ActivityModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ApplyMaintainModels
{
    public class ApplyMaintainListViewModel
    {
        private ApplyMaintainListFilterModel m_filter = new ApplyMaintainListFilterModel();
        public ApplyMaintainListFilterModel Filter { get { return m_filter; } set { m_filter = value; } }
        private ApplyMaintainListResultModel m_result = new ApplyMaintainListResultModel();
        public ApplyMaintainListResultModel Result { get { return m_result; } set { m_result = value; } }


    }
}