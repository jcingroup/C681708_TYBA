using OutWeb.Entities;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.TypeManageModels
{
    /// <summary>
    /// 最新消息列表資回傳模型
    /// </summary>
    public class TypeManageListResultModel : IPaginationModel
    {
        private List<TypeManageListDataModel> m_data = new List<TypeManageListDataModel>();
        public List<TypeManageListDataModel> Data { get { return this.m_data; } set { this.m_data = value; } }

        /// <summary>
        /// 分頁模型
        /// </summary>
        public PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}