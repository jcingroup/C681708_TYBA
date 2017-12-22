using System;

namespace OutWeb.Models.FrontEnd.NewsFrontEndModels
{
    public class NewsListFrontFilterModel
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
        /// 分類代碼
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 發稿日期(起)
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// 發稿日期(訖)
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}