using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ApplyMaintainModels
{
    public class ApplyMaintainListDataModel
    {
        public int ID { get; set; }
        public string PublishDateString { get; set; }
        public string ActivityName { get; set; }
        public string ActivityDateRange { get; set; }
        public bool ApplyStatus { get; set; }
        public double? Sort { get; set; }

        /// <summary>
        /// 已報名隊伍 不分組別
        /// </summary>
        public int RegisteredCount { get; set; }

        /// <summary>
        /// 限制報名隊伍 不分組別
        /// </summary>
        public int LimitCount { get; set; }
    }
}