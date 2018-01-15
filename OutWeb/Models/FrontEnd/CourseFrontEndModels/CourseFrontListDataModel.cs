using System;

namespace OutWeb.Models.FrontEnd.CourseFrontEndModels
{
    public class CourseFrontListDataModel
    {
        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 網址位址
        /// </summary>
        public string UrlAddr { get; set; }
        public double Sort { get; set; }
        public DateTime PublishDate { get; set; }
    }
}