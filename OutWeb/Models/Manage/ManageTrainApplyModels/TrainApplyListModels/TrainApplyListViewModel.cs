using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageTrainApplyModels
{
    public class TrainApplyListViewModel
    {
        TrainApplyListFilterModel m_filter = new TrainApplyListFilterModel();
        TrainApplyListResultModel m_result = new TrainApplyListResultModel();

        public TrainApplyListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public TrainApplyListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}