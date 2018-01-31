namespace OutWeb.Models.Manage.ActivityModels
{
    public class ApplyMaintainListFilterModel
    {
        /// <summary>
        /// 排序條件
        /// </summary>
        public string SortColumn { get; set; }
        /// <summary>
        /// 選取頁面
        /// </summary>
        public int CurrentPage { get; set; }
    }
}