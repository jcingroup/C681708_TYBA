using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Models.Manage.ImgModels;
using OutWeb.Models.Manage.ManageNewsModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using OutWeb.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 通知管理模組
    /// </summary>
    public class NotificationListModule:IDisposable
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }



        public object DoGetList<TFilter>(TFilter model, Language language)
        {
            NotificationListFilterModel filterModel = (model as NotificationListFilterModel);
            PublicMethodRepository.FilterXss(filterModel);
            NotificationListResultModel result = new NotificationListResultModel();
            List<SMS_EMAIL_FAX_LIST> data = new List<SMS_EMAIL_FAX_LIST>();
            try
            {
                data = DB.SMS_EMAIL_FAX_LIST.ToList();
                //語系搜尋
                //if (!language.Equals(Language.NotSet))
                //{
                //    this.NewsListFilterLanguage(language, ref data);
                //}
                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }
                //發布起始日期搜尋
                if (!string.IsNullOrEmpty(filterModel.DelieverStartDate) || !string.IsNullOrEmpty(filterModel.DelieverEndDate))
                {
                    this.ListDateFilter(filterModel.DelieverStartDate,filterModel.DelieverEndDate,ref data);
                }

                ////發布結束日期搜尋
                //if (!string.IsNullOrEmpty(filterModel.SendEndDate))
                //{
                //    this.ListDateFilter(Convert.ToDateTime(filterModel.SendEndDate), ref data);
                //}

                //分類
                if (!string.IsNullOrEmpty(filterModel.Type))
                {
                    this.ListTypeFilter(filterModel.Type, ref data);
                }

                //狀態
                if (!string.IsNullOrEmpty(filterModel.Status))
                {
                    this.ListStatusFilter(filterModel.Status, ref data);
                }

                //排序
                this.ListSort(filterModel.SortColumn, filterModel.Status, ref data);

                PaginationResult pagination;
                //分頁
                this.ListPageList(filterModel.CurrentPage, ref data, out pagination);
                result.Pagination = pagination;
                foreach (var d in data)
                    PublicMethodRepository.HtmlDecode(d);

                result.Data = data;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }


        /// <summary>
        /// 列表類型搜尋
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="data"></param>

        private void ListTypeFilter(string typeCode, ref List<SMS_EMAIL_FAX_LIST> data)
        {
            data = data.Where(s => s.STYPE == typeCode).ToList();
        }

        /// <summary>
        /// 列表狀態搜尋
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="data"></param>

        private void ListStatusFilter(string typeCode, ref List<SMS_EMAIL_FAX_LIST> data)
        {
            data = data.Where(s => s.STATUS.ToString() == typeCode).ToList();
        }


        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<SMS_EMAIL_FAX_LIST> data)
        {
            var r = data.Where(s => s.TITLE.Contains(filterStr)).ToList();
            data = r;
        }



        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="startDateString"></param>
        /// <param name="endDateString"></param>
        /// <param name="data"></param>
        private void ListDateFilter(String startDateString, String endDateString, ref List<SMS_EMAIL_FAX_LIST> data)
        {
            var r = data.ToList();

            if (!string.IsNullOrEmpty(startDateString))
            {
                DateTime startDate = DateTime.Parse(startDateString).Date;
                r=r.Where(s => s.DELIEVER_DATE >= startDate).ToList();
            }

            if (!String.IsNullOrEmpty(endDateString))
            {
                DateTime endDate = DateTime.Parse(endDateString).Date;
                r =r.Where(s => s.DELIEVER_DATE <= endDate).ToList();
            }

            data = r;
        }


        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<SMS_EMAIL_FAX_LIST> data, out PaginationResult pagination)
        {
            int pageSize = (int)PageSizeConfig.SIZE10;
            int startRow = (currentPage - 1) * pageSize;
            PaginationResult paginationResult = new PaginationResult()
            {
                CurrentPage = currentPage,
                DataCount = data.Count,
                PageSize = pageSize,
                FirstPage = 1,
                LastPage = Convert.ToInt32(Math.Ceiling((decimal)data.Count / pageSize))
            };
            pagination = paginationResult;
            var query = data.Skip(startRow).Take(pageSize).ToList();
            data = query;
        }

        /// <summary>
        /// 列表排序功能
        /// </summary>
        /// <param name="sortCloumn"></param>
        /// <param name="data"></param>
        private void ListSort(string sortCloumn, string status, ref List<SMS_EMAIL_FAX_LIST> data)
        {
            switch (sortCloumn)
            {
                case "sortStatus/desc":
                    data = data.OrderByDescending(o => o.STATUS).ToList();
                    break;

                case "sortStatus/asc":
                    data = data.OrderBy(o => o.STATUS).ToList();
                    break;

                case "sortType/desc":
                    data = data.OrderByDescending(o => o.STYPE).ToList();
                    break;

                case "sortType/asc":
                    data = data.OrderBy(o => o.STYPE).ToList();
                    break;

                case "sortDelieverDate/desc":
                    data = data.OrderByDescending(o => o.DELIEVER_DATE).ToList();
                    break;

                case "sortDelieverDate/asc":
                    data = data.OrderBy(o => o.DELIEVER_DATE).ToList();
                    break;

                case "sortInsertDate/desc":
                    data = data.OrderByDescending(o => o.INSERT_DATE).ToList();
                    break;

                case "sortInsertDate/asc":
                    data = data.OrderBy(o => o.INSERT_DATE).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.ID).ToList();
                    break;
            }
        }
        public void Dispose()
        {
            if (this.DB.Database.Connection.State == System.Data.ConnectionState.Open)
            {
                this.DB.Database.Connection.Close();
            }
            this.DB.Dispose();
            this.DB = null;
        }


    }
}