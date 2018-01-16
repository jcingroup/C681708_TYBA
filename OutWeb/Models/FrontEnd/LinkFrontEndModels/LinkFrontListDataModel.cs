using OutWeb.Models.Manage;
using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.LinkFrontEndModels
{
    public class LinkFrontListDataModel
    {
        public List<FileViewModel> Image { get { return this.m_image; } set { this.m_image = value; } }
        private List<FileViewModel> m_image = new List<FileViewModel>();

        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 網址位址
        /// </summary>
        public string UrlAddr { get; set; }
    }
}