namespace OutWeb.Models.Manage.QuestionnairesModels
{
    /// <summary>
    /// 最新消息列表資過濾條件模型
    /// </summary>
    public class QuestionListFilterModel
    {
        /// <summary>
        /// 選取頁面
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 是否需要登入
        /// </summary>
        public string IsLogin { get; set; }

        /// <summary>
        /// 查詢關鍵字
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// 排序條件
        /// </summary>
        public string SortColumn { get; set; }

        /// <summary>
        /// 開始日期
        /// </summary>
        public string BeginDateStr { get; set; }

        /// <summary>
        /// 結束日期
        /// </summary>
        public string EndDateStr { get; set; }
    }
}