using OutWeb.Models.Manage.BannerModels;
using OutWeb.Models.Manage.ManageNewsModels;
using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.HomeMultipleModels
{
    public class HomeFrontListDataModel
    {
        private NewsListResultModel m_news = new NewsListResultModel();
        public NewsListResultModel News { get { return m_news; } set { m_news = value; } }

        private List<BannerDetailsModel> m_banner = new List<BannerDetailsModel>();
        public List<BannerDetailsModel> Banner { get { return m_banner; } set { m_banner = value; } }

    }
}