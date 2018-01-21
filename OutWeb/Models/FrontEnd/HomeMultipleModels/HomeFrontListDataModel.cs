using OutWeb.Models.FrontEnd.CourseFrontEndModels;
using OutWeb.Models.FrontEnd.NewsFrontEndModels;

namespace OutWeb.Models.FrontEnd.HomeMultipleModels
{
    public class HomeFrontListDataModel
    {
        private NewsListFrontViewModel m_news = new NewsListFrontViewModel();
        public NewsListFrontViewModel News { get { return m_news; } set { m_news = value; } }

        private CourseListFrontViewModel m_course = new CourseListFrontViewModel();
        public CourseListFrontViewModel Course { get { return m_course; } set { m_course = value; } }
    }
}