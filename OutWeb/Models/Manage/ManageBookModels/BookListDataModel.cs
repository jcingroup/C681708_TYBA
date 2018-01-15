using OutWeb.Repositories;
using System;

namespace OutWeb.Models.Manage.ManageBookModels
{
    /*
    public int 主索引 { get; set; }
    public int 對應分類索引 { get; set; }
    public System.DateTime 發稿時間 { get; set; }
    public string 發稿人 { get; set; }
    public string 名稱 { get; set; }
    public string 摘要 { get; set; }
    public Nullable<double> 排序 { get; set; }
    public bool 顯示狀態 { get; set; }
    public System.DateTime 建立時間 { get; set; }
    */
    /// <summary>
    /// 最新消息列表資料模型
    /// </summary>
    public class BookListDataModel
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
        /// 分類索引
        /// </summary>
        public int TypeID { get; set; }

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 前台顯示
        /// </summary>
        public bool Status { get; set; }

        public string StatusStr
        {
            get
            {
                string str = string.Empty;
                if (this.Status)
                    str = "顯示";
                else
                    str = "隱藏";
                return str;
            }
        }


        /// <summary>
        /// 發布日期
        /// </summary>
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// 發布日期
        /// </summary>
        public DateTime CreateDate { get; set; }


        /// <summary>
        /// 排序
        /// </summary>
        public double? Sort { get; set; }

    }
}