using OutWeb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageNewsModels
{
    /// <summary>
    /// 最新消息列表資回傳模型
    /// </summary>
    public class NewsListResultModel: IPaginationModel
    {
        List<新聞> m_data = new List<新聞>();
        public List<新聞> Data { get { return this.m_data; } set { this.m_data = value; } }

        /// <summary>
        /// 分頁模型
        /// </summary>
        public PaginationResult m_pagination = new PaginationResult();
        public PaginationResult Pagination { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}