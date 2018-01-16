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
        /// Action名稱
        /// </summary>
        public string ActionName { get; set; }
        public FileUploadType UploadIdentify { get { return m_uploadIdentify; } set { m_uploadIdentify = value; } }
        FileUploadType m_uploadIdentify = FileUploadType.NOTSET;

        /// <summary>
        /// 檔案容器
        /// </summary>

        private List<FileViewModel> m_memberDataMultiple = new List<FileViewModel>();
        public List<FileViewModel> MemberDataMultiple { get { return m_memberDataMultiple; } set { this.m_memberDataMultiple = value; } }
    }

}