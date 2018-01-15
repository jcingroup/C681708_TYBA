using System;

namespace OutWeb.Models.FrontEnd.NewsFrontEndModels
{
    public class NewsFrontListDataModel
    {

        public int SiteCount { get; set; }
        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 排序欄位
        /// </summary>
        public double? SortIndex { get; set; }

        /// <summary>
        /// 顯示狀態
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 首頁顯示
        /// </summary>
        public bool DisplayHome { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 分類
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 新聞連結
        /// </summary>
        public string NewsUrl { get; set; }

        /// <summary>
        /// 發布日期
        /// </summary>
        public string PublishDateStr
        { get { return this.PublishDate.ToString("yyyy\\/MM\\/dd"); } }

        /// <summary>
        /// 發布日期
        /// </summary>
        public DateTime PublishDate
        { get; set; }
    }
}