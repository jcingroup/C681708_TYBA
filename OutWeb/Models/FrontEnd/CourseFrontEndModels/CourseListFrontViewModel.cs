using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.CourseFrontEndModels
{
    public class CourseListFrontViewModel 
    {
        private CourseListFrontResultModel m_result = new CourseListFrontResultModel();
        public CourseListFrontResultModel Result { get { return m_result; } set { m_result = value; } }
   
    }
}