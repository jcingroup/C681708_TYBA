namespace OutWeb.Models.Manage.ManageLinkModels
{
    public class LinkListDataModel
    {
        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        public string Title { get; set; }
        public string UrlLink { get; set; }

        public bool Status { get; set; }

        public double Sort { get; set; }
    }
}