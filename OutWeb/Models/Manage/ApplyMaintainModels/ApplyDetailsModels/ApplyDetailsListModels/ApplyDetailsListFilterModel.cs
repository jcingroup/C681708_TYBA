namespace OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyDetailsListModels
{
    public class ApplyDetailsListFilterModel
    {
        public int? GroupID { get; set; }
        /// <summary>
        /// 排序條件
        /// </summary>
        public string SortColumn { get; set; }
        /// <summary>
        /// 選取頁面
        /// </summary>
        public int CurrentPage { get; set; }
        public string QueryString { get; set; }
    }
}