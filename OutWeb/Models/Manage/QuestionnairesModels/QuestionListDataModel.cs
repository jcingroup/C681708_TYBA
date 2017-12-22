using System;

namespace OutWeb.Models.Manage.QuestionnairesModels
{
    /// <summary>
    /// 最新消息列表資料模型
    /// </summary>
    public class QuestionListDataModel
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
        /// 是否上架
        /// </summary>
        public bool Status { get; set; }

        public string StatusStr
        {
            get
            {
                string str = string.Empty;
                if (this.Status)
                    str = "上架";
                else
                    str = "下架";
                return str;
            }
        }

        /// <summary>
        /// 是否上架
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
        /// 開始日
        /// </summary>
        public string BeginDateStr { get; set; }

        /// <summary>
        /// 結束日
        /// </summary>
        public string EndDateStr { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public double Sort { get; set; }

       public DateTime CreateDate { get; set; }
    }
}