using System;

namespace OutWeb.Models.FrontEnd.TrainModels.TrainListModels
{
    public class TrainListDataModel
    {
        /*
        活動日期	課程名稱	已報名人數/上限	線上報名
        */
        public int ID { get; set; }
        public int ApplyID { get; set; }
        public string ActivityDateStr { get; set; }
        public DateTime ActivityDateBegin { get; set; }
        public DateTime ActivityDateEnd { get; set; }
        public string ActivityName { get; set; }
        /// <summary>
        /// 報名參加人數
        /// </summary>
        public int AlreadyRegistered { get; set; }
        /// <summary>
        /// 報名完成人數
        /// </summary>
        public int Completed { get; set; }
        /// <summary>
        /// 額滿候補人數
        /// </summary>
        public int FullOfWaiting { get; set; }
        public int LimitCount { get; set; }
        public bool ActivityStatus { get; set; }
        /// <summary>
        /// 報名日期起
        /// </summary>
        public DateTime RegistrationBeginDate { get; set; }
        /// <summary>
        /// 報名日期迄
        /// </summary>
        public DateTime RegistrationEndDate { get; set; }
        /// <summary>
        /// 報名狀態描述 報名or 報名截止
        /// </summary>
        public string RegistrationStatusDescription { get; set; }
        /// <summary>
        /// 報名狀態描述 報名or 報名截止
        /// </summary>
        public bool IsStopRegistering { get; set; }
        /// <summary>
        /// 所屬用戶識別碼
        /// </summary>
        public string MapUserAccount { get; set; }
        /// <summary>
        /// 是否需要費用
        /// </summary>
        public bool ChargesStatus { get; set; }
        public string ChargesStatusString { get { return ChargesStatus ? "繳費" : "免費"; } }

        public double Sort { get; set; }
    }
}