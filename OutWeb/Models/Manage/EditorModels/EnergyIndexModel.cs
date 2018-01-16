using System.Collections.Generic;

namespace OutWeb.Models.Manage.EditorModels
{
    /// <summary>
    /// 能源查核申報系統資料模型
    /// </summary>
    public class EnergyIndexModel : Editor, IManage
    {
        private List<FileViewModel> m_filesData = new List<FileViewModel>();
        private List<FileViewModel> m_imagesData = new List<FileViewModel>();

        public List<FileViewModel> FilesData
        {
            get
            {
                return m_filesData;
            }

            set
            {
                m_filesData = value;
            }
        }

        public List<FileViewModel> ImagesData
        {
            get
            {
                return m_imagesData;
            }

            set
            {
                m_imagesData = value;
            }
        }
    }
}