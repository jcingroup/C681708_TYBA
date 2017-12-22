using System;

namespace OutWeb.Models.FrontEnd.BookFrontEndModels
{
    public class BookFrontFilterModel
    {
        /// <summary>
        /// 查詢關鍵字
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// 分類
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 發布日期
        /// </summary>
        public DateTime PublishBeginDate { get; set; }

        public DateTime PublishEndDate { get; set; }
    }
}