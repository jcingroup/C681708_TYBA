using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.QuestionnairesModels
{
    public class QuestionListViewModel
    {
        QuestionListFilterModel m_filter = new QuestionListFilterModel();
        QuestionListResultModel m_result = new QuestionListResultModel();

        public QuestionListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public QuestionListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}