using OutWeb.Entities;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageLinkModels
{
    public class LinkDetailsDataModel
    {
        /// <summary>
        /// 圖片
        /// </summary>
        public List<MemberViewModel> Image { get { return this.m_image; } set { this.m_image = value; } }
        private List<MemberViewModel> m_image = new List<MemberViewModel>();

        private 外部連結 m_details = new 外部連結();

        public 外部連結 Data
        { get { return this.m_details; } set { this.m_details = value; } }
    }
}