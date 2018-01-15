using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageTrainModels
{
    public class TrainDetailsDataModel
    {
        /// <summary>
        /// 檔案
        /// </summary>
        public List<MemberViewModel> FilesData { get { return this.m_filesData; } set { this.m_filesData = value; } }
        private List<MemberViewModel> m_filesData = new List<MemberViewModel>();

        private TrainStandardDataModel m_details = new TrainStandardDataModel();

        public TrainStandardDataModel Data
        { get { return this.m_details; } set { this.m_details = value; } }
    }
}