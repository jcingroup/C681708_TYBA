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
        public bool ApplyStatus { get; set; }
        public double? Sort { get; set; }
    }
}