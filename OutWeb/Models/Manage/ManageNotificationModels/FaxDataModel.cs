using OutWeb.Entities;
using OutWeb.Models.Manage.FileModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageNotificationModels
{
    public class FaxDataModel
    {
        private List<FileViewModel> m_memberFile = new List<FileViewModel>();
        private List<FileViewModel> m_attachmentFile = new List<FileViewModel>();

        /// <summary>
        /// 收件人
        /// </summary>
        public List<FileViewModel> MemberFile { get { return this.m_memberFile; } set { this.m_memberFile = value; } }

        /// <summary>
        /// 附件上傳
        /// </summary>
        public List<FileViewModel> AttachmentFiles { get { return this.m_attachmentFile; } set { this.m_attachmentFile = value; } }

        private FAX m_fax = new FAX();

        public FAX Data
        { get { return this.m_fax; } set { this.m_fax = value; } }
    }
}