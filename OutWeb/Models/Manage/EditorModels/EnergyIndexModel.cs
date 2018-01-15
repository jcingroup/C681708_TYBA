using System.Collections.Generic;

namespace OutWeb.Models.Manage.EditorModels
{
    /// <summary>
    /// 能源查核申報系統資料模型
    /// </summary>
    public class EnergyIndexModel : Editor, IManage
    {
        private List<MemberViewModel> m_filesData = new List<MemberViewModel>();
        private List<MemberViewModel> m_imagesData = new List<MemberViewModel>();

        public List<MemberViewModel> FilesData
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

        public List<MemberViewModel> ImagesData
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