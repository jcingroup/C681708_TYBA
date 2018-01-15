using OutWeb.Enums;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.FileModels
{
    public class FilesModel
    {
        private List<int> m_oldFileIds = new List<int>();
        public List<int> OldFileIds { get { return this.m_oldFileIds; } set { this.m_oldFileIds = value; } }

        public int ID { get; set; }

        /// <summary>
        /// 上傳模式 M:多張 S:單張
        /// </summary>
        //public string UploadType { get; set; }

        /// <summary>
        /// Action名稱
        /// </summary>
        public string ActionName { get; set; }
        public FileUploadType UploadIdentify { get { return m_uploadIdentify; } set { m_uploadIdentify = value; } }
        FileUploadType m_uploadIdentify = FileUploadType.NOTSET;
        /// <summary>
        /// 單筆
        /// </summary>
        private List<MemberViewModel> m_memberData = new List<MemberViewModel>();
        public List<MemberViewModel> MemberData { get { return m_memberData; } set { this.m_memberData = value; } }
        /// <summary>
        /// 多筆
        /// </summary>

        private List<MemberViewModel> m_memberDataMultiple = new List<MemberViewModel>();
        public List<MemberViewModel> MemberDataMultiple { get { return m_memberDataMultiple; } set { this.m_memberDataMultiple = value; } }
    }

}