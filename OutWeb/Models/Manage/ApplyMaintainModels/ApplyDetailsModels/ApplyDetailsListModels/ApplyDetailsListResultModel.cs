using OutWeb.Entities;
using OutWeb.Models.Manage.ApplyMaintainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyDetailsListModels
{
    public class ApplyDetailsListResultModel : IPaginationModel
    {
        List<ApplyDetailsListDataModel> m_data = new List<ApplyDetailsListDataModel>();
        public List<ApplyDetailsListDataModel> Data { get { return m_data; } set { m_data = value; } }
        /// <summary>
        /// 分頁模型
        /// </summary>
        public PaginationResult m_pagination = new PaginationResult();
        public PaginationResult Pagination { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}