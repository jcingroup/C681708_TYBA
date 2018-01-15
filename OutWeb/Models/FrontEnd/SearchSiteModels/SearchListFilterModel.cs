namespace OutWeb.Models.FrontEnd.SearchSiteModels
{
    public class SearchListFilterModel
    {
        /// <summary>
        /// 選取頁面
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 查詢關鍵字
        /// </summary>
        public string QueryString { get; set; }
    }
}