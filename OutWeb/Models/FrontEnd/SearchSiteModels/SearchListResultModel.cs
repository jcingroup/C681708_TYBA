using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.SearchSiteModels
{
    public class SearchListResultModel
    {
        List<SearchListDataModel> m_Data = new List<SearchListDataModel>();
        public List<SearchListDataModel> Data { get { return m_Data; } set { m_Data = value; } }

        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}