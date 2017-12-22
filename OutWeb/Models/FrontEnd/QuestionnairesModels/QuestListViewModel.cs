using System;
using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.QuestionnairesModels
{
    public class QuestListViewModel : IPaginationModel
    {
        private QuestionFilterData m_filter = new QuestionFilterData();
        public QuestionFilterData Filter { get { return m_filter; } set { m_filter = value; } }
        private List<QuestionListData> m_data = new List<QuestionListData>();
        public List<QuestionListData> Data { get { return m_data; } set { m_data = value; } }
        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }

    public class QuestionFilterData
    {
        public int Page { get; set; }
    }

    public class QuestionListData
    {
        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 是否上架(開放)
        /// </summary>
        public bool Status { get; set; }

        public string IsFinishOrIsNotYetStr
        {
            get
            {
                string str = string.Empty;
                if (this.IsFinish)
                    str = "已結束";
                else
                {
                    if (this.IsNotyet)
                        str = "未開放";
                    else
                        str = "開放";
                }

                return str;
            }
        }

        /// <summary>
        /// 是否已經結束
        /// </summary>
        public bool IsFinish
        {
            get; set;
        }
        /// <summary>
        /// 尚未開放
        /// </summary>
        public bool IsNotyet
        { get; set; }

        /// <summary>
        /// 是否需要登入
        /// </summary>
        public bool IsLogin { get; set; }

        /// <summary>
        /// 是否需要登入
        /// </summary>

        public string IsLoginStr
        {
            get
            {
                string str = string.Empty;
                if (this.IsLogin)
                    str = "是";
                else
                    str = "否";
                return str;
            }
        }

        /// <summary>
        /// 填寫人數
        /// </summary>
        public int PeopleNumber { get; set; }

        /// <summary>
        /// 建立日期 字串
        /// </summary>
        public string CreateDateStr { get { return this.CreateDate.ToString("yyyy\\/MM\\/dd"); } }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public double Sort { get; set; }
    }
}