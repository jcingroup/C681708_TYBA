namespace OutWeb.Models.Manage.CasesModels
{
    public class CasesListViewModel
    {
        private CasesListFilterModel m_filter = new CasesListFilterModel();
        private CasesListResultModel m_result = new CasesListResultModel();

        public CasesListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public CasesListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}