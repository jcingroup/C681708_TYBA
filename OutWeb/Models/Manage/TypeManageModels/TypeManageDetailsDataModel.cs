using OutWeb.Entities;
using System;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.TypeManageModels
{
    public class TypeManageDetailsDataModel
    {
        public int? FilterTypeID { get; set; }
        public string FilterTypeName { get; set; }
        /// <summary>
        /// 主索引
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 分類對應檔索引
        /// </summary>
        public int MappingID { get; set; }
        /// <summary>
        /// 自訂代碼
        /// </summary>
        public int CustomID { get; set; }

        /// <summary>
        /// 分類下的產品數量
        /// </summary>
        //public int ProductCount { get; set; }

        /// <summary>
        /// 產品分類名稱
        /// </summary>
        public string TypeName { get; set; }

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
        /// 備註
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public double Sort { get; set; }

        /// <summary>
        /// 語系
        /// </summary>
        public string Language { get; set; }

        private List<TypeManageDetailsDataModel> m_subTypeList = new List<TypeManageDetailsDataModel>();
        public List<TypeManageDetailsDataModel> SubTypeList { get { return m_subTypeList; } set { m_subTypeList = value; } }
    }
}