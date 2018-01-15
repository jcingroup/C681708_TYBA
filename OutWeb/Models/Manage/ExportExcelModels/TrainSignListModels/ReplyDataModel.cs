using OutWeb.Models.Manage.ManageTrainApplyModels.TrainApplyDetailsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ExportExcelModels.TrainSignListModels
{
    public class ReplyDataModel : ReplyBase
    {
        List<Data> m_data = new List<Data>();
        public List<Data> Data { get { return m_data; } set { m_data = value; } }
    }

    public class Data
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

        private List<TrainApplyParticipants> m_participants = new List<TrainApplyParticipants>();
        public List<TrainApplyParticipants> ParticipantsData { get { return m_participants; } set { m_participants = value; } }


    }


}