using OutWeb.Models.FrontEnd.QuestionnairesModels;
using System;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.QuestionnairesModels
{
    public class QuestionDetailsDataModel
    {
        /// <summary>
        /// 圖片
        /// </summary>

        private QuestionDetailsData m_details = new QuestionDetailsData();

        public QuestionDetailsData Data
        { get { return this.m_details; } set { this.m_details = value; } }

        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string OpeningTime { get; set; }
        public string EndTime { get; set; }
        public string Status { get; set; }
        public string IsSignIn { get; set; }
        public int SendCount { get; set; }
        public double Sort { get; set; }
        public string Url { get; set; }

        public DateTime CreateDate { get; set; }

    }


    public class QuestionDetailsData
    {
        private List<Topic> m_topic = new List<QuestionnairesModels.Topic>();
        public List<Topic> Topic
        {
            set { m_topic = value; }
            get { return m_topic; }
        }
    }

    public class Topic
    {
        public int ID { get; set; }
        public double Sort { get; set; }
        public string Required { get; set; }
        public int TopicType { get; set; }
        public string TopicTypeName { get; set; }
        public string TopicContent { get; set; }
        private List<Option> m_option = new List<QuestionnairesModels.Option>();
        public List<Option> Option { get { return m_option; } set { m_option = value; } }
        public bool IsCanDelete { get; set; }

        public int? BeforeID { get; set; }

        List<QuestionDetailsAnswerModel> m_answer = new List<QuestionDetailsAnswerModel>();
        public List<QuestionDetailsAnswerModel> Answer { get { return m_answer; } set { m_answer = value; } }
    }

    public class Option
    {
        public int OptionID { get; set; }
        public string OptionValue { get; set; }
        public double Sort { get; set; }
        public bool IsCanDeleteOption { get; set; }
        public int? BeforeOptionID { get; set; }


    }
}