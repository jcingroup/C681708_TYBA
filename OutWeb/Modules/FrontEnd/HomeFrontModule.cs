using OutWeb.Entities;
using OutWeb.Models.FrontEnd.HomeMultipleModels;
using OutWeb.Models.Manage.BannerModels;
using OutWeb.Models.Manage.ManageNewsModels;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutWeb.Modules.FrontEnd
{
    public class HomeFrontModule : IDisposable
    {
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public HomeFrontListDataModel GetList()
        {
            HomeFrontListDataModel model = new HomeFrontListDataModel();
            model.Banner = GetBannerList();
            model.News = GetNewsList();
            return model;
        }

        private List<BannerDetailsModel> GetBannerList()
        {
            List<BannerDetailsModel> results = new List<BannerDetailsModel>();
            List<BANNER> data = new List<BANNER>();
            try
            {
                data = DB.BANNER
                    .Where(o => o.DISABLE == false)
                    .OrderByDescending(o => o.BUD_DT).ThenByDescending(g => g.SQ)
                    .ToList();
                FileModule fileModule = new FileModule();
                using (var bannerModule = new BannerModule())
                {
                    foreach (var d in data)
                    {
                        PublicMethodRepository.HtmlDecode(d);
                        BannerDetailsModel temp = bannerModule.DoGetDetailsByID(d.ID);
                        temp.Files = fileModule.GetFiles((int)d.ID, "Banner", "F"); 
                        results.Add(temp);
                    }
                }
                fileModule.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return results;
        }

        private NewsListResultModel GetNewsList()
        {
            NewsListResultModel result = new NewsListResultModel();
            List<NEWS> data = new List<NEWS>();
            try
            {
                data = DB.NEWS
                    .Where(o => o.DISABLE == false && o.HOME_PAGE_DISPLAY == true)
                    .OrderByDescending(o => o.PUB_DT_STR).ThenByDescending(g => g.SQ)
                    .ToList()
                    .Take(5).ToList();

                foreach (var d in data)
                    PublicMethodRepository.HtmlDecode(d);

                result.Data = data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public void Dispose()
        {
            if (this.DB.Database.Connection.State == System.Data.ConnectionState.Open)
            {
                this.DB.Database.Connection.Close();
            }
            this.DB.Dispose();
            this.DB = null;
        }
    }
}