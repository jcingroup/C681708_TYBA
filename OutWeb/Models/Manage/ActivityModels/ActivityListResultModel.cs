using OutWeb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ActivityModels
{
    public class ActivityListResultModel : IPaginationModel
    {
        List<OLACT> m_data = new List<OLACT>();
        public List<OLACT> Data { get { return m_data; } set { m_data = value; } }
        private Dictionary<int, bool> m_validActivityStatusBuffer = new Dictionary<int, bool>();
        public Dictionary<int, bool> ValidActivityStatusBuffer { get { return m_validActivityStatusBuffer; } set { m_validActivityStatusBuffer = value; } }
        /// <summary>
        /// 分頁模型
        /// </summary>
        public PaginationResult m_pagination = new PaginationResult();
        public PaginationResult Pagination { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}