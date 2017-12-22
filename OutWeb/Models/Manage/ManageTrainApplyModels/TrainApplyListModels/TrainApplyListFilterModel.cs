namespace OutWeb.Models.Manage.ManageTrainApplyModels
{
    /// <summary>
    /// 最新消息列表資過濾條件模型
    /// </summary>
    public class TrainApplyListFilterModel
    {
        /// <summary>
        /// 選取頁面
        /// </summary>
        public int CurrentPage { get; set; }


        /// <summary>
        /// 查詢關鍵字
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// 排序條件
        /// </summary>
        public string SortColumn { get; set; }

        /// <summary>
        /// 活動日期(起)
        /// </summary>
        public string ActivityBeginDate { get; set; }

        /// <summary>
        /// 活動日期(訖)
        /// </summary>
        public string ActivityEndDate { get; set; }
    }
}