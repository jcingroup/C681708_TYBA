
namespace OutWeb.Models.FrontEnd.TrainModels.TrainApplyModels
{
    /// <summary>
    /// 研討會報名模型
    /// </summary>
    public class TrainApplyViewModel
    {
        /// <summary>
        /// 研討會基本資料模型
        /// </summary>
        public TrainContentModel TrainInfo { get { return m_trainInfo; } set { m_trainInfo = value; } }
        private TrainContentModel m_trainInfo = new TrainContentModel();
        /// <summary>
        /// 研討會報名模型
        /// </summary>
        public TrainApplyDataModel Apply { get { return m_apply; } set { m_apply = value; } }
        private TrainApplyDataModel m_apply = new TrainApplyDataModel();

    }
}