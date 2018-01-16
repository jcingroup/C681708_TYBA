using OutWeb.Entities;
using OutWeb.Models.Manage.FileModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageNotificationModels
{
    public class SMSDataModel : IManage
    {
        private List<FileViewModel> m_filesData = new List<FileViewModel>();
        private List<FileViewModel> m_imagesData = new List<FileViewModel>();
        /// <summary>
        /// 附件檔案
        /// </summary>
        public List<FileViewModel> FilesData { get { return this.m_filesData; } set { this.m_filesData = value; } }
        /// <summary>
        /// 圖片
        /// </summary>
        public List<FileViewModel> ImagesData { get { return this.m_imagesData; } set { this.m_imagesData = value; } }

        private SMS m_sms = new SMS();

        public SMS Data
        { get { return this.m_sms; } set { this.m_sms = value; } }
    }
}