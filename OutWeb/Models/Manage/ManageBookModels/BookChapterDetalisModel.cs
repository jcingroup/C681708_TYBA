using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageBookModels
{
    public class BookChapterDetalisModel
    {
        public int? ID { get; set; }

        public int MapFileID { get; set; }
        public string Alias { get; set; }
        public float? SQ { get; set; }
    }
}