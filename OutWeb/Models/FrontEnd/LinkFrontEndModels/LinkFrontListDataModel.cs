using OutWeb.Models.Manage;
using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.LinkFrontEndModels
{
    public class LinkFrontListDataModel
    {
        public List<MemberViewModel> Image { get { return this.m_image; } set { this.m_image = value; } }
        private List<MemberViewModel> m_image = new List<MemberViewModel>();

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