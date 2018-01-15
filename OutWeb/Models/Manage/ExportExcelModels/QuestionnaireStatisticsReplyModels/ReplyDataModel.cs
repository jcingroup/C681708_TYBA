using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ExportExcelModels.QuestionnaireStatisticsReplyModels
{
    public class ReplyDataModel: ReplyBase
    {
        /// <summary>
        /// 問卷ID
        /// </summary>
        public int QuestionnairesID { get; set; }
        /// <summary>
        /// 問卷名稱
        /// </summary>
        public string QuestionnairesName { get; set; }

        List<QuestionnairesReplyDetails> m_details = new List<QuestionnairesReplyDetails>();
        public List<QuestionnairesReplyDetails> Details { get { return m_details; } set { m_details = value; } }
    }

    public class QuestionnairesReplyDetails
    {
        /// <summary>
        /// 題目ID
        /// </summary>
        public int TopicID { get; set; }
        /// <summary>
        /// 題目內容
        /// </summary>
        public string TopicContent { get; set; }
        /// <summary>
        /// 題目類型名稱
        /// </summary>
        public string TopicTypeName { get; set; }
        /// <summary>
        /// 填寫人
        /// </summary>
        public string TopicAnswerUser { get; set; }
        public int? TopicAnswerID { get; set; }
        /// <summary>
        /// 回答題目-項目序號(適用於 多選 單選)
        /// </summary>
        public int? TopicAnswerItemNumber { get; set; }
        /// <summary>
        /// 回答題目-項目內容(適用於 多選 單選)
        /// </summary>
        public string TopicAnswerItemContent { get; set; }
        /// <summary>
        /// 回答題目-項目文字內容(適用於 多問答)
        /// </summary>
        public string TopicAnswerTextContent { get; set; }
        /// <summary>
        ///  回答日期
        /// </summary>
        public System.DateTime AnswerDate { get; set; }
    }
}