using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.NewsFrontEndModels
{
    public class NewsListFrontResultModel : IPaginationModel
    {
        private List<NewsFrontListDataModel> m_data = new List<NewsFrontListDataModel>();
        public List<NewsFrontListDataModel> Data { get { return m_data; } set { m_data = value; } }

        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}