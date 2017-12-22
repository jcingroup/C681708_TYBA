using OutWeb.Entities;
using OutWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.CasesModels
{
    /// <summary>
    /// 產品列表資回傳模型
    /// </summary>
    public class CasesListResultModel : IPaginationModel
    {
        List<能源案例> m_data = new List<能源案例>();
        public List<能源案例> Data { get { return this.m_data; } set { this.m_data = value; } }

        /// <summary>
        /// 分頁模型
        /// </summary>
        public PaginationResult m_pagination = new PaginationResult();
        public PaginationResult Pagination { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}