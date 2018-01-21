using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.BannerModels
{
    public class BannerListViewModel
    {
       BannerFilterModel m_filter = new BannerFilterModel();
        BannerListResultModel m_result = new BannerListResultModel();

        public BannerFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public BannerListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}