using OutWeb.Repositories;
using System;

namespace OutWeb.Models.Manage.CasesModels
{
    /// <summary>
    /// 最新消息列表資料模型
    /// </summary>
    public class CasesListDataModel
    {
        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 案例名稱
        /// </summary>
        public string WorksName { get; set; }

        /// <summary>
        /// 所屬分類索引
        /// </summary>
        public int TypeID { get; set; }

        /// <summary>
        /// 所屬分類
        /// </summary>

        /// <summary>
        /// 成立日期
        /// </summary>

        public string PublishDateStr { get; set; }

   
        /// <summary>
        /// 成立日期 10碼字串格式
        /// </summary>
        public DateTime? WorksBulidDateStr { get; set; }

        /// <summary>
        /// 前台顯示
        /// </summary>
        public bool DisplayForFront { get; set; }

        public string DisplayForFrontStr
        {
            get
            {
                string str = string.Empty;
                if (this.DisplayForFront)
                    str = "顯示";
                else
                    str = "隱藏";
                return str;
            }
        }
        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }

        /// <summary>
        /// 語系
        /// </summary>
        public string Language { get; set; }
    }
}