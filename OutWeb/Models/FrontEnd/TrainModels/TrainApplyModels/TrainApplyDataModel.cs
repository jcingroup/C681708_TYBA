using OutWeb.Enums;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.TrainModels.TrainApplyModels
{
    public class TrainApplyDataModel
    {
        public int? ID { get; set; }
        public bool Agree { get; set; }
        public string UserNo { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Birthday { get; set; }
        public string MobilePhone { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyFax { get; set; }
        public string BusinessNo { get; set; }
        public string BusinessNoTitle { get; set; }

        public DateTime UpdateTime { get; set; }
        private List<Participants> m_participants = new List<Participants>();
        public List<Participants> ParticipantsData { get { return m_participants; } set { m_participants = value; } }
    }

    public class Participants
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set; }
        public string DietTypeValue { get; set; }

        public DietCategory DietType
        {
            get
            {
                DietCategory result = PublicMethodRepository.GetEnumByValue<DietCategory>(DietTypeValue);
                return result;
            }
            set
            {
                DietTypeValue = DietType.ToString();
            }
        }

        public bool IsContactPerson { get; set; }
        /// <summary>
        /// 報名狀態
        /// </summary>
        public int StatusCode { get; set; }

        public string StatusDescription
        {
            get
            {
                string str = StatusCode == 0 ? "取消報名" : StatusCode == 1 ? "報名成功" : StatusCode == 2 ? "額滿候補" : "";
                return str;
            }
        }

        public string StatusCodeFromUser { get; set; }

        public int? BeforeID { get; set; }
    }
}