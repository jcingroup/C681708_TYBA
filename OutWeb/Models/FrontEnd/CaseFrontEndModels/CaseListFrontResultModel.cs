using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.CaseFrontEndModels
{
    public class CaseListFrontResultModel : IPaginationModel
    {
        Dictionary<string, List<CaseFrontListDataModel>> m_groupData = new Dictionary<string, List<CaseFrontListDataModel>>();
        public Dictionary<string, List<CaseFrontListDataModel>> GroupData { get { return m_groupData; } set { m_groupData = value; } }
         List<CaseFrontListDataModel> m_listDatas = new List<CaseFrontListDataModel>();
        public List<CaseFrontListDataModel> ListData { get { return m_listDatas; } set { m_listDatas = value; } }

        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}