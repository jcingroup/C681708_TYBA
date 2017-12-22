using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.SearchSiteModels
{
    public class SearchListDataModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string LinkAddr { get; set; }
        public int Type { get; set; }
        public DateTime UpDateTime { get; set; }
    }
}