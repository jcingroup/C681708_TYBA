using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.CaseFrontEndModels
{
    public class CaseListFrontViewModel 
    {
        private CaseListFrontResultModel m_result = new CaseListFrontResultModel();
        public CaseListFrontResultModel Result { get { return m_result; } set { m_result = value; } }
   
    }
}