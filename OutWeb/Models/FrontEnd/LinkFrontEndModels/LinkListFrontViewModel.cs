using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.LinkFrontEndModels
{
    public class LinkListFrontViewModel 
    {
        private LinkListFrontResultModel m_result = new LinkListFrontResultModel();
        public LinkListFrontResultModel Result { get { return m_result; } set { m_result = value; } }
   
    }
}