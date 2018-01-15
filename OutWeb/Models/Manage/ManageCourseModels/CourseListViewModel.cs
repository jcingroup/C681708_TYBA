using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageCourseModels
{
    public class CourseListViewModel
    {
        CourseListFilterModel m_filter = new CourseListFilterModel();
        CourseListResultModel m_result = new CourseListResultModel();

        public CourseListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public CourseListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}