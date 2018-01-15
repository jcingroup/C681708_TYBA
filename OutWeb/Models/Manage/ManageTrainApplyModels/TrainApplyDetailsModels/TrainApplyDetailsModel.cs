using OutWeb.Models.Manage.ManageTrainApplyModels.TrainApplyDetailsModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageTrainApplyModels
{
    public class TrainApplyDetailsModel
    {
        /*
        課程名稱XXXX課程(台中場)活動日期2017/11/7
        人數上限70已報名/完成繳費60 / 50便當(葷/素)40 / 20
        */
        private TrainApplyStandardDataModel m_data = new TrainApplyStandardDataModel();

        public TrainApplyStandardDataModel Data
        { get { return this.m_data; } set { this.m_data = value; } }

        private List<TrainApplyDataListModel> m_list = new List<TrainApplyDataListModel>();
        public List<TrainApplyDataListModel> List { get { return m_list; } set { m_list = value; } }

    }
}