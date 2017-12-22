using OutWeb.Models.Manage;
using System;
using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.TrainModels.TrainApplyModels
{
    public class TrainContentModel
    {
        /*
        活動地點	工研院中興院區/新竹縣竹東鎮中興路4段195號‧78館209室
        主辦單位	經濟部能源局
        協辦單位	工研院綠能與環境研究所
        報名期限	2017/6/20 上午 09:30:00 至 2017/6/27 下午 05:00:00
        聯絡人	〇〇〇
        活動時間	2017/10/21 13:30~16:00
        聯絡電話	03-5910085
        備註
        報名人數限制	80
        活動內容(編輯器)
        附件1 附件2
        */

        private List<MemberViewModel> m_filesData = new List<MemberViewModel>();

        /// <summary>
        /// 圖片
        /// </summary>
        public List<MemberViewModel> FilesData { get { return this.m_filesData; } set { this.m_filesData = value; } }

        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///   活動名稱
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        ///   活動地點
        /// </summary>
        public string ActivityLocation { get; set; }

        /// <summary>
        ///   活動日期
        /// </summary>
        public string ActivityDate { get; set; }

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
        //public string ActivityTimeRange { get; set; }

        public bool ChargesStatus { get; set; }
        public string ChargesStatusStr { get { return ChargesStatus ? "繳費" : "免費"; } }
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
        /// 發稿時間
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 以活動日期判斷是否已截止報名
        /// </summary>
        public bool IsStopRegistering
        {
            get; set;

        }

    }
}