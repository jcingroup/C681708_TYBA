using OutWeb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.DownloadModels
{
    public class DownloadListResultModel : IPaginationModel
    {
        List<DLFILES> m_data = new List<DLFILES>();
        public List<DLFILES> Data { get { return m_data; } set { m_data = value; } }

        /// <summary>
        /// 分頁模型
        /// </summary>
        public PaginationResult m_pagination = new PaginationResult();
        public PaginationResult Pagination { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}