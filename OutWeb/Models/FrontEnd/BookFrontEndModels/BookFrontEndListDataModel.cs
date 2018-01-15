using OutWeb.Models.Manage;
using System;
using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.BookFrontEndModels
{
    public class BookFrontEndListDataModel
    {
        private List<BookFrontEndDataModel> m_data = new List<BookFrontEndDataModel>();
        public List<BookFrontEndDataModel> Result { get { return this.m_data; } set { this.m_data = value; } }
        private BookFrontFilterModel m_filter = new BookFrontFilterModel();
        public BookFrontFilterModel Filter { get { return m_filter; } set { m_filter = value; } }
    }

    public class BookFrontEndDataModel
    {
        private List<MemberViewModel> m_coverImg = new List<MemberViewModel>();

        /// <summary>
        /// 封面圖
        /// </summary>
        public List<MemberViewModel> CoverImg { get { return this.m_coverImg; } set { this.m_coverImg = value; } }

        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 排序欄位
        /// </summary>
        public double? SortIndex { get; set; }

        /// <summary>
        /// 狀態代碼
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 分類
        /// </summary>
        public int Type { get; set; }

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