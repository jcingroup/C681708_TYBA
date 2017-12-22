using OutWeb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageTrainModels
{
    /// <summary>
    /// 最新消息列表資回傳模型
    /// </summary>
    public class TrainListResultModel: IPaginationModel
    {
        List<TrainStandardDataModel> m_data = new List<TrainStandardDataModel>();
        public List<TrainStandardDataModel> Data { get { return this.m_data; } set { this.m_data = value; } }

        /// <summary>
        /// 分頁模型
        /// </summary>
        public PaginationResult m_pagination = new PaginationResult();
        public PaginationResult Pagination { get { return this.m_pagination; } set { this.m_pagination = value; } }
    }
}