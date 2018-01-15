using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.HomeMultipleModels
{
    public class HomeListFrontResultModel : IPaginationModel
    {
        HomeFrontListDataModel m_groupData = new HomeFrontListDataModel();
        public HomeFrontListDataModel GroupData { get { return m_groupData; } set { m_groupData = value; } }

        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}