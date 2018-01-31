using OutWeb.Models.Manage.ActivityModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyDetailsListModels
{
    public class ApplyDetailsListViewModel
    {
        private ApplyDetailsListFilterModel m_filter = new ApplyDetailsListFilterModel();
        public ApplyDetailsListFilterModel Filter { get { return m_filter; } set { m_filter = value; } }
        private ApplyDetailsListResultModel m_result = new ApplyDetailsListResultModel();
        public ApplyDetailsListResultModel Result { get { return m_result; } set { m_result = value; } }


    }
}