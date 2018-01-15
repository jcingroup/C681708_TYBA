using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageTrainModels
{
    public class TrainListViewModel
    {
        TrainListFilterModel m_filter = new TrainListFilterModel();
        TrainListResultModel m_result = new TrainListResultModel();

        public TrainListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public TrainListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}