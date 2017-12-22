using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models
{
    public class PaginationResult
    {
        private int m_PageSize = 0, m_currPage = 0, m_dataCount = 0;
        private int m_firstPage = 0, m_lastPage = 0;
        internal PaginationResult() { }
        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize
        {
            internal set { m_PageSize = value; }
            get { return m_PageSize; }
        }
        /// <summary>
        /// 當前頁數
        /// </summary>
        public int CurrentPage
        {
            internal set { m_currPage = value; }
            get { return m_currPage; }
        }

        /// <summary>
        /// 上一頁
        /// </summary>
        public int PrePage
        {
            get
            {
                int p = (this.CurrentPage - 1);
                if (p <= 1)
                    p = 1;
                return p;
            }
        }

        /// <summary>
        /// 下一頁
        /// </summary>
        public int NextPage
        {
            get
            {
                int p = (this.CurrentPage + 1);
                if (p > LastPage)
                    p = LastPage;
                return p;
            }
        }
        /// <summary>
        /// 資料總筆數
        /// </summary>
        public int DataCount
        {
            internal set { m_dataCount = value; }
            get { return m_dataCount; }
        }
        /// <summary>
        /// 提供顯示的起始頁數
        /// </summary>
        public int FirstPage
        {
            get { return m_firstPage; }
            internal set { m_firstPage = value; }
        }
        /// <summary>
        /// 提供顯示的結束頁數
        /// </summary>
        public int LastPage
        {
            get { return m_lastPage; }
            internal set { m_lastPage = value; }
        }
    }
}