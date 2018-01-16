using OutWeb.Models.Manage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.EditModels
{
    public class EditorFront
    {
        private List<FileViewModel> m_filesData = new List<FileViewModel>();
        public List<FileViewModel> FilesData { get { return m_filesData; } set { m_filesData = value; } }
        public string Content { get; set; }
    }
}