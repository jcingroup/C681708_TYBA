using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.TrainModels.TrainListModels
{
    public class TrainListResultModel : IPaginationModel
    {
        private List<TrainListDataModel> m_data = new List<TrainListDataModel>();
        public List<TrainListDataModel> Data { get { return m_data; } set { m_data = value; } }

        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}