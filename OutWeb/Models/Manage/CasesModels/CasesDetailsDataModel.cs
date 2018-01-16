using OutWeb.Entities;
using OutWeb.Models.Manage.ImgModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.CasesModels
{
    public class CasesDetailsDataModel : IManage
    {
        private List<FileViewModel> m_filesData = new List<FileViewModel>();
        private List<FileViewModel> m_imagesData = new List<FileViewModel>();

        /// <summary>
        /// 圖片
        /// </summary>
        public List<FileViewModel> FilesData { get { return this.m_filesData; } set { this.m_filesData = value; } }

        public List<FileViewModel> ImagesData { get { return this.m_imagesData; } set { this.m_imagesData = value; } }

        private 能源案例 m_details = new 能源案例();

        public 能源案例 Data
        { get { return this.m_details; } set { this.m_details = value; } }
    }
}