using OutWeb.Entities;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageLinkModels
{
    public class LinkDetailsDataModel
    {
        /// <summary>
        /// 圖片
        /// </summary>
        public List<FileViewModel> Image { get { return this.m_image; } set { this.m_image = value; } }
        private List<FileViewModel> m_image = new List<FileViewModel>();

        private 外部連結 m_details = new 外部連結();

        public 外部連結 Data
        { get { return this.m_details; } set { this.m_details = value; } }
    }
}