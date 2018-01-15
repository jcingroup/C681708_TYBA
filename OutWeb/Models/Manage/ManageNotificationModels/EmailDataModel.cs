﻿using OutWeb.Entities;
using OutWeb.Models.Manage.FileModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageNotificationModels
{
    public class EmailDataModel
    {
        private List<MemberViewModel> m_memberFile = new List<MemberViewModel>();
        private List<MemberViewModel> m_attachmentFile = new List<MemberViewModel>();

        /// <summary>
        /// 收件人
        /// </summary>
        public List<MemberViewModel> MemberFile { get { return this.m_memberFile; } set { this.m_memberFile = value; } }

        /// <summary>
        /// 附件上傳
        /// </summary>
        public List<MemberViewModel> AttachmentFiles { get { return this.m_attachmentFile; } set { this.m_attachmentFile = value; } }

        private EMAIL m_mail = new EMAIL();

        public EMAIL Data
        { get { return this.m_mail; } set { this.m_mail = value; } }
    }
}