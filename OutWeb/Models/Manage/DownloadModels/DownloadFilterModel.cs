namespace OutWeb.Models.Manage.DownloadModels
{
    public class DownloadFilterModel
    {
        /// <summary>
        /// 選取頁面
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 上下架
        /// </summary>
        public bool Disable { get; set; }

        /// <summary>
        /// 分類
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 查詢關鍵字
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// 排序條件
        /// </summary>
        public string SortColumn { get; set; }

        /// <summary>
        /// 發布日期
        /// </summary>
        public string PublishDate { get; set; }
    }
}