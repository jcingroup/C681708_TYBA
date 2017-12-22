using OutWeb.Models.Manage.QuestionnairesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.QuestionnairesModels
{
    public class QuestionAnswerModel
    {

        private QuestionDetailsDataModel m_viewData = new QuestionDetailsDataModel();
        public QuestionDetailsDataModel ViewData { get { return m_viewData; } set { m_viewData = value; } }
    }

    public class QuestionDetailsAnswerModel
    {
        public int QuestionID { get; set; }
        public int QuestionTypeID { get; set; }
        /// <summary>
        /// 前台排序號碼 不在資料內
        /// </summary>
        public int Index { get; set; }
        public string Value { get; set; }

        public bool Required { get; set; }

    }
}