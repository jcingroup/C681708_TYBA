using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageLinkModels
{
    public class LinkListViewModel
    {
        LinkListFilterModel m_filter = new LinkListFilterModel();
        LinkListResultModel m_result = new LinkListResultModel();

        public LinkListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public LinkListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}