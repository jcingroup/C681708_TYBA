using OutWeb.Enums;
using OutWeb.Repositories;
using System;

namespace OutWeb.Models.Manage.TypeManageModels
{
    /// <summary>
    /// 最新消息列表資料模型
    /// </summary>
    public class TypeManageListDataModel
    {

        public int MappingID { get; set; }
        private string m_mappingItemName = string.Empty;

        public string MappingItemName { get { return m_mappingItemName; } set { m_mappingItemName = value; } }

        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 分類下的產品數量
        /// </summary>
        //public int ProductCount { get; set; }

        /// <summary>
        /// 產品分類名稱
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 
        /// </summary>

        ///// <summary>
        ///// 狀態
        ///// </summary>
        //public string StatusCode { get; set; }

        ///// <summary>
        ///// 狀態描述
        ///// </summary>
        //public string StatusDescription
        //{
        //    get
        //    {
        //        var eStatus = PublicMethodRepository.GetEnumByValue<DisplayEnums>(this.StatusCode);
        //        return PublicMethodRepository.GetEnumDescription(eStatus);
        //    }
        //}

        /// <summary>
        /// 停用
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public double Sort { get; set; }

        /// <summary>
        /// 語系
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CreateDate { get; set; }
    }
}