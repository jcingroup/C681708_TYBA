using OutWeb.Entities;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageBookModels
{
    public class BookDetailsDataModel
    {
        private List<MemberViewModel> m_chapterFiles = new List<MemberViewModel>();
        private List<MemberViewModel> m_fullBookFile = new List<MemberViewModel>();
        private List<MemberViewModel> m_coverImg = new List<MemberViewModel>();
        private List<BookChapterDetalisModel> m_chapterDetails = new List<BookChapterDetalisModel>();

        /// <summary>
        /// 封面圖
        /// </summary>
        public List<MemberViewModel> CoverImg { get { return this.m_coverImg; } set { this.m_coverImg = value; } }

        /// <summary>
        /// 全文
        /// </summary>
        public List<MemberViewModel> FullBookFile { get { return this.m_fullBookFile; } set { this.m_fullBookFile = value; } }

        /// <summary>
        /// 章節
        /// </summary>
        public List<MemberViewModel> ChapterFiles { get { return this.m_chapterFiles; } set { this.m_chapterFiles = value; } }

        public List<BookChapterDetalisModel> ChapterDetails { get { return this.m_chapterDetails; } set { this.m_chapterDetails = value; } }

        private 出版品主檔 m_details = new 出版品主檔();

        public 出版品主檔 Data
        { get { return this.m_details; } set { this.m_details = value; } }
    }
}