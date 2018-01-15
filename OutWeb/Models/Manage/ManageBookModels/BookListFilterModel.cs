using System;

namespace OutWeb.Models.Manage.ManageBookModels
{
    /// <summary>
    /// 最新消息列表資過濾條件模型
    /// </summary>
    public class BookListFilterModel
    {
        /// <summary>
        /// 選取頁面
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 查詢狀態
        /// </summary>
        public string Status { get; set; }

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
        public DateTime PublishBeginDate { get; set; }
        public DateTime PublishEndDate { get; set; }
    }
}