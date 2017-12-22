namespace OutWeb.Models.FrontEnd.CaseFrontEndModels
{
    public class CaseFrontListDataModel
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
        /// 分類名稱
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 分類
        /// </summary>
        public int Type { get; set; }
    }
}