using System.Collections.Generic;

namespace OutWeb.Models.Manage.QuestionnaireStatisticsModels
{
    public class QuestionnaireStatisticsDetailsDataModel
    {
        /// <summary>
        /// 圖片
        /// </summary>

        private List<TopicStatistic> m_topicStatistics = new List<TopicStatistic>();

        public List<TopicStatistic> TopicStatistics
        { get { return this.m_topicStatistics; } set { this.m_topicStatistics = value; } }

        public int ID { get; set; }

        /// <summary>
        /// 問卷名稱
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 發送量
        /// </summary>
        public int SendCount { get; set; }

        /// <summary>
        /// 回收量
        /// </summary>
        public int RecoveredCount { get; set; }

        /// <summary>
        /// 回覆率
        /// </summary>
        public double Responseate { get; set; }
    }

    /// <summary>
    /// 題目統計模型
    /// </summary>
    public class TopicStatistic
    {
        /// <summary>
        /// 題目主索引
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 題目名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 題型ID
        /// </summary>
        public int TypeID { get; set; }
        /// <summary>
        /// 題型名稱
        /// </summary>
        public string TypeName { get; set; }

        private List<ReplyOptionModel> m_replyModel = new List<QuestionnaireStatisticsModels.ReplyOptionModel>();

        public List<ReplyOptionModel> ReplyList
        { get { return m_replyModel; } set { m_replyModel = value; } }

        /// <summary>
        /// 回覆量
        /// </summary>
        public int ReplyCount { get; set; }

        /// <summary>
        /// 回覆率
        /// </summary>
        public double Responseate { get; set; }
        /// <summary>
        /// 回覆比例(直方圖)
        /// </summary>
        public string Proportion { get; set; }
    }

    /// <summary>
    /// 回覆選項模型
    /// </summary>
    public class ReplyOptionModel
    {
        /// <summary>
        /// 選項主索引
        /// </summary>
        public int OptionID { get; set; }
        /// <summary>
        /// 選項名稱
        /// </summary>
        public string OptionName { get; set; }

        /// <summary>
        /// 回覆比例(直方圖)
        /// </summary>
        public string Proportion { get; set; }

        /// <summary>
        /// 回覆量
        /// </summary>
        public int ReplyCount { get; set; }

        /// <summary>
        /// 回覆率
        /// </summary>
        public double Responseate { get; set; }
    }
}