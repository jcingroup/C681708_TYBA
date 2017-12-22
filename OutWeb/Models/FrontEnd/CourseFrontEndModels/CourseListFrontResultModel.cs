using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.CourseFrontEndModels
{
    public class CourseListFrontResultModel : IPaginationModel
    {
        private List<CourseFrontListDataModel> m_data = new List<CourseFrontListDataModel>();
        public List<CourseFrontListDataModel> Data { get { return m_data; } set { m_data = value; } }

        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}