namespace OutWeb.Models.Manage.CasesModels
{
    /// <summary>
    /// 最新消息列表資過濾條件模型
    /// </summary>
    public class CasesListFilterModel
    {
        /// <summary>
        /// 選取頁面
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// 狀態ID
        /// </summary>
        public int? TypeID1 { get; set; }
        public int? TypeID2 { get; set; }
        /// <summary>
        /// 發布日期
        /// </summary>
        public string PublishDate { get; set; }
        /// <summary>
        /// 查詢狀態
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 前台顯示
        /// </summary>
        public string DisplayForFrontEnd { get; set; }

        /// <summary>
        /// 查詢關鍵字
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// 排序條件
        /// </summary>
        public string SortColumn { get; set; }
    }
}