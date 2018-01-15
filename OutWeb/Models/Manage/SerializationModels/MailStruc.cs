using System;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.SerializationModels
{
    /// <summary>
    /// 發信JSON結構最上層物件
    /// </summary>
    public class MailStruc
    {
        private string m_id = Guid.NewGuid().ToString();
        /// <summary>
        /// Guid即可
        /// </summary>
        public string id { get { return m_id; } set { m_id = value; } }

        /// <summary>
        /// 自訂資訊，非必要欄位。
        /// </summary>
        public string tag { get; set; }

        public DateTime createtime { get; set; }
        public MailInfo mailinfo { get; set; }
        public IList<SendResult> sendresult { get; set; }
        public DateTime? sendDateTime { get; set; }
    }

    public class MailInfo
    {
        public string from { get; set; }
        public string[] to { get; set; }
        public string[] bcc { get; set; }
        public string[] cc { get; set; }
        public string subject { get; set; }
        public string body { get; set; }

        /// <summary>
        /// e.g. http://www.jcin.com.tw/test.docx
        /// </summary>
        public string[] filepath { get; set; }
    }

    public class SendResult
    {
        public DateTime datetime { get; set; }
        public bool result { get; set; }
    }
}