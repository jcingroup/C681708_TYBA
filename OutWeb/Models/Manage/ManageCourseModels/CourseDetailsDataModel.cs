using OutWeb.Entities;
using OutWeb.Models.Manage.FileModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageCourseModels
{
    public class CourseDetailsDataModel : IManage
    {
        private List<MemberViewModel> m_filesData = new List<MemberViewModel>();
        private List<MemberViewModel> m_imagesData = new List<MemberViewModel>();

        /// <summary>
        /// 圖片
        /// </summary>
        public List<MemberViewModel> FilesData { get { return this.m_filesData; } set { this.m_filesData = value; } }

        public List<MemberViewModel> ImagesData { get { return this.m_imagesData; } set { this.m_imagesData = value; } }

        private 課程 m_details = new 課程();

        public 課程 Data
        { get { return this.m_details; } set { this.m_details = value; } }
    }
}