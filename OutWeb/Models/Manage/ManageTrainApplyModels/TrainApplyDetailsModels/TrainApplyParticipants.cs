using OutWeb.Enums;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageTrainApplyModels.TrainApplyDetailsModels
{
    public class TrainApplyParticipants
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
        /// 是否繳費
        /// </summary>
        public bool IsCharges { get; set; }
        /// <summary>
        /// 報名狀態
        /// </summary>
        public int StatusCode { get; set; }
    }

}