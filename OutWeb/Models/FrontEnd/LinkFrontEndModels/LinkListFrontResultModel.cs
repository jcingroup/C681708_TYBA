using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.LinkFrontEndModels
{
    public class LinkListFrontResultModel : IPaginationModel
    {
        private List<LinkFrontListDataModel> m_data = new List<LinkFrontListDataModel>();
        public List<LinkFrontListDataModel> Data { get { return m_data; } set { m_data = value; } }

        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}