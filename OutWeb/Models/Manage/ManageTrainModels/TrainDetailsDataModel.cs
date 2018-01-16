using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageTrainModels
{
    public class TrainDetailsDataModel
    {
        /// <summary>
        /// 檔案
        /// </summary>
        public List<FileViewModel> FilesData { get { return this.m_filesData; } set { this.m_filesData = value; } }
        private List<FileViewModel> m_filesData = new List<FileViewModel>();

        private TrainStandardDataModel m_details = new TrainStandardDataModel();

        public TrainStandardDataModel Data
        { get { return this.m_details; } set { this.m_details = value; } }
    }
}