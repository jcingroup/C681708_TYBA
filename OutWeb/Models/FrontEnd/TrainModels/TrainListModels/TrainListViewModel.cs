using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.TrainModels.TrainListModels
{
    public class TrainListViewModel 
    {
        private TrainListFilterModel m_filter = new TrainListFilterModel();
        public TrainListFilterModel Filter { get { return m_filter; } set { m_filter = value; } }
        private TrainListResultModel m_result = new TrainListResultModel();
        public TrainListResultModel Result { get { return m_result; } set { m_result = value; } }
   
    }
}