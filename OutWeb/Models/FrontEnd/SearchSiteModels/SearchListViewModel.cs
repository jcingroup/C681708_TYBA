namespace OutWeb.Models.FrontEnd.SearchSiteModels
{
    public class SearchListViewModel
    {
        private SearchListResultModel m_result = new SearchListResultModel();
        public SearchListResultModel Result { get { return m_result; } set { m_result = value; } }

        private SearchListFilterModel m_filter = new SearchListFilterModel();
        public SearchListFilterModel Filter { get { return m_filter; } set { m_filter = value; } }
    }
}