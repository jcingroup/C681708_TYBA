using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.QuestionnaireStatisticsModels
{
    public class QuestionnaireStatisticsListViewModel
    {
        QuestionnaireStatisticsListFilterModel m_filter = new QuestionnaireStatisticsListFilterModel();
        QuestionnaireStatisticsListResultModel m_result = new QuestionnaireStatisticsListResultModel();

        public QuestionnaireStatisticsListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public QuestionnaireStatisticsListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}