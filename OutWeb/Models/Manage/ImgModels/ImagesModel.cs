using System.Collections.Generic;

namespace OutWeb.Models.Manage.ImgModels
{
    public class ImagesModel
    {
        private List<int> m_oldImageIds = new List<int>();
        public List<int> OldImageIds { get { return this.m_oldImageIds; } set { this.m_oldImageIds = value; } }

        public int ID { get; set; }

        /// <summary>
        /// 上傳模式 M:多張 S:單張
        /// </summary>
        //public string UploadType { get; set; }

        /// <summary>
        /// Action名稱
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// 單筆
        /// </summary>

        private List<FileViewModel> m_memberData = new List<FileViewModel>();
        public List<FileViewModel> MemberData { get { return m_memberData; } set { this.m_memberData = value; } }
        /// <summary>
        /// 多筆
        /// </summary>

        private List<FileViewModel> m_memberDataMultiple = new List<FileViewModel>();
        public List<FileViewModel> MemberDataMultiple { get { return m_memberDataMultiple; } set { this.m_memberDataMultiple = value; } }
    }
}