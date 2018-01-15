using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageTrainApplyModels.TrainApplyDetailsModels
{
    public class TrainApplyViewModel
    {
        public int TrainID { get; set; }
        public int ApplyID { get; set; }
        List<ParticipantsData> m_participantsData = new List<TrainApplyDetailsModels.ParticipantsData>();
        public List<ParticipantsData> Participants { get { return m_participantsData; }set { m_participantsData = value; } }

    }

    public class ParticipantsData
    {
        public int ID { get; set; }
        public int Status { get; set; }
        public int ChargesStatus { get; set; }
    }
}