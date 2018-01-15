using System;

namespace OutWeb.Models.FrontEnd.TrainModels.TrainListModels
{
    public class TrainListFilterModel
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
        /// 發布日期
        /// </summary>
        public string PublishDate { get; set; }
        /// <summary>
        /// 活動日期
        /// </summary>
        public string ActivityDate { get; set; }

        /// <summary>
        /// 活動日期(起)
        /// </summary>
        public DateTime BeginDate { get; set; }
        /// <summary>
        /// 活動日期(訖)
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}