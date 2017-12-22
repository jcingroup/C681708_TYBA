namespace OutWeb.Models.Manage.QuestionnaireStatisticsModels
{
    /// <summary>
    /// 最新消息列表資料模型
    /// </summary>
    public class QuestionnaireStatisticsListDataModel
    {
        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 問卷標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 填寫時間
        /// </summary>
        public string ValidDate { get; set; }

        /// <summary>
        /// 發出量
        /// </summary>
        public int SendCount { get; set; }
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