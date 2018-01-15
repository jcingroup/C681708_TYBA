using System;

namespace OutWeb.Models.Manage.ManageTrainModels
{
    public class TrainStandardDataModel
    {
        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 葷食數量
        /// </summary>
        public int MeatCount { get; set; }

        /// <summary>
        /// 素食數量
        /// </summary>
        public int Vegetarian { get; set; }

        /// <summary>
        ///   活動名稱
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        ///   活動地點
        /// </summary>
        public string ActivityLocation { get; set; }

        /// <summary>
        ///   活動日期起
        /// </summary>
        public DateTime ActivityDateBegin { get; set; }
        /// <summary>
        /// 活動日期訖
        /// </summary>
        public DateTime ActivityDateEnd { get; set; }

        /// <summary>
        ///   主辦單位
        /// </summary>
        public string Organizer { get; set; }

        /// <summary>
        ///   協辦單位
        /// </summary>
        public string CoOrganiser { get; set; }

        /// <summary>
        ///   報名期限(起)
        /// </summary>
        public string DeadlineBegin { get; set; }

        /// <summary>
        ///   報名期限(迄)
        /// </summary>
        public string DeadlineEnd { get; set; }

        /// <summary>
        ///  活動時間範圍
        /// </summary>
        public string ActivityTimeRange { get; set; }

        /// <summary>
        ///   聯絡人
        /// </summary>
        public string ContactPerson { get; set; }

        /// <summary>
        ///   聯絡電話
        /// </summary>
        public string ContactPhoneNumber { get; set; }

        /// <summary>
        ///   備註
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        ///   活動人數上限
        /// </summary>
        public int EnrollmentRestrictions { get; set; }
        /// <summary>
        ///   用戶參加人數限制
        /// </summary>
        public int SingleEnrollmentRestrictions { get; set; }


        /// <summary>
        ///   活動內容
        /// </summary>
        public string ActivityContent { get; set; }

        /// <summary>
        ///  活動狀態
        /// </summary>
        public bool ActivityStatus { get; set; }
        /// <summary>
        /// 報名狀態
        /// </summary>
        public bool RegistrationStatus { get; set; }
        /// <summary>
        /// 首頁顯示
        /// </summary>
        public bool DisplayHome { get; set; }

        /// <summary>
        /// 收費狀態 (0免費 1收費)
        /// </summary>
        public bool ChargesStatus { get; set; }

        /// <summary>
        /// 發稿時間
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 排序
        /// </summary>

        public double Sort { get; set; }
    }
}