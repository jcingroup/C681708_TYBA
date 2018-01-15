using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.HomeMultipleModels
{
    public class HomeListFrontViewModel 
    {
        private HomeListFrontResultModel m_result = new HomeListFrontResultModel();
        public HomeListFrontResultModel Result { get { return m_result; } set { m_result = value; } }
   
    }
}