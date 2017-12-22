using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models
{
    /// <summary>
    /// 分頁介面模型 繼承此介面都必須實作分頁功能
    /// </summary>
    public interface IPaginationModel
    {
        /// <summary>
        /// 分頁
        /// </summary>
        PaginationResult Pagination { get; set; }
    }
}