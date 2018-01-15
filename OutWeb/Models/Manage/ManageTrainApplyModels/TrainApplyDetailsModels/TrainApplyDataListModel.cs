using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageTrainApplyModels.TrainApplyDetailsModels
{
    public class TrainApplyDataListModel
    {
        /*
        報名序號	公司名稱	公司電話	姓名(聯絡人)	參加人數	報名完成	額滿候補	繳費狀況
        */


        /// <summary>
        /// 報名檔主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 對應課程主檔ID
        /// </summary>
        public int MapTrainID { get; set; }
        /// <summary>
        /// 公司名稱
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 公司電話
        /// </summary>
        public string CompanyPhone { get; set; }

        /// <summary>
        ///   聯絡人
        /// </summary>
        public string ContactPerson { get; set; }

        /// <summary>
        /// 參加人數
        /// </summary>
        public int RegistrationCount { get; set; }

        /// <summary>
        /// 報名完成人數
        /// </summary>
        public int RegistrationSuccessCount { get; set; }



        /// <summary>
        /// 額滿候補人數
        /// </summary>
        public int RegistrationAlternateCount { get; set; }

        /// <summary>
        /// 排序
        /// </summary>

        public double Sort { get; set; }

        /// <summary>
        /// 繳費狀況
        /// </summary>
        public string PaymentStatus { get; set; }
    }
}