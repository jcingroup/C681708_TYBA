using System.Collections.Generic;

namespace OutWeb.Models.FrontEnd.ApplyModels
{
    public class ApplyListViewModel: IPaginationModel
    {
        private List<ApplyListViewDataModel> m_listData = new List<ApplyListViewDataModel>();
        public List<ApplyListViewDataModel> ListData { get { return m_listData; } set { m_listData = value; } }
        public int Page { get; set; }

        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }

    public class ApplyListViewDataModel
    {
        public int? ID { get; set; }

        public string ActivityDateTimeDescription { get; set; }
        public string Title { get; set; }
        public string ApplyDateRange { get; set; }
        public string Remark { get; set; }
        /// <summary>
        /// 已報名人數
        /// </summary>
        public int Registered { get; set; }
        /// <summary>
        /// 報名組別限制
        /// </summary>
        public int GroupApplyLimit { get; set; }
        /// <summary>
        /// 報名狀態 Filter: 1.當日已過報名日期 2.可報名組別數量以滿
        /// </summary>
        public bool ActivityStatus { get; set; }

        public double? Sort { get; set; }
    }
}