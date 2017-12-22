using OutWeb.Models.Manage.ManageTrainApplyModels.TrainApplyDetailsModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageTrainApplyModels
{
    public class TrainApplyDataModel
    {
        /// <summary>
        /// 研討會主檔索引
        /// </summary>
        public int TrainID { get; set; }
        /// <summary>
        /// 報名主檔索引
        /// </summary>
        public int? ID { get; set; }

        public string UserNo { get; set; }
        public string CompanyName { get; set; }

        public string Email { get; set; }

        public string CompanyPhone { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }

        public bool ChargesStatus { get; set; }


        private List<TrainApplyParticipants> m_participants = new List<TrainApplyParticipants>();
        public List<TrainApplyParticipants> ParticipantsData { get { return m_participants; } set { m_participants = value; } }
    }
}