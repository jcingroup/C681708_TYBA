using OutWeb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.BannerModels
{
    public class BannerListResultModel : IPaginationModel
    {
        List<BANNER> m_data = new List<BANNER>();
        public List<BANNER> Data { get { return m_data; } set { m_data = value; } }

        /// <summary>
        /// 分頁模型
        /// </summary>
        public PaginationResult m_pagination = new PaginationResult();
        public PaginationResult Pagination { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}