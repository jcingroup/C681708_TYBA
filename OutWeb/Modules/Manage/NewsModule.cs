﻿using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.ManageNewsModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 最新消息列表模組
    /// </summary>
    public class NewsModule : IDisposable
    {
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        public void DoDeleteByID(int ID)
        {
            var data = this.DB.NEWS.Where(s => s.ID == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("[刪除比賽訊息] 查無此訊息，可能已被移除");
            try
            {
                this.DB.NEWS.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除比賽訊息]" + ex.Message);
            }
        }

        public NewsDetailsDataModel DoGetDetailsByID(int ID)
        {
            NewsDetailsDataModel result = new NewsDetailsDataModel();
            NEWS data = DB.NEWS.Where(w => w.ID == ID).FirstOrDefault();
            PublicMethodRepository.HtmlDecode(data);
            result.Data = data;
            return result;
        }

        public NewsListResultModel DoGetList(NewsListFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);
            NewsListResultModel result = new NewsListResultModel();
            List<NEWS> data = new List<NEWS>();
            try
            {
                data = DB.NEWS.ToList();

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }

                //發佈日期搜尋
                if (!string.IsNullOrEmpty(filterModel.PublishDate))
                {
                    this.ListDateFilter(filterModel.PublishDate, ref data);
                }

                //首頁顯示
                if (!string.IsNullOrEmpty(filterModel.DisplayForHomePage))
                {
                    this.ListStatusFilter(filterModel.DisplayForHomePage, "DisplayHome", ref data);
                }

                //上下架
                if (!string.IsNullOrEmpty(filterModel.Disable))
                {
                    this.ListStatusFilter(filterModel.Disable, "Disable", ref data);
                }

                //排序
                this.ListSort(filterModel.SortColumn, ref data);
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

        public int DoSaveData(FormCollection form, int? ID = null)
        {
            NEWS saveModel;

            if (ID == 0)
            {
                saveModel = new NEWS();
                saveModel.BUD_ID = UserProvider.Instance.User.ID;
                saveModel.BUD_DT = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.NEWS.Where(s => s.ID == ID).FirstOrDefault();
            }
            saveModel.TITLE = form["title"];
            saveModel.HOME_PAGE_DISPLAY = form["disHome"] == "on" ? true : false;
            saveModel.DISABLE = form["disable"] == null ? false : Convert.ToBoolean(form["disable"]);
            saveModel.SQ = form["sortIndex"] == null ? 1 : Convert.ToDouble(form["sortIndex"]);
            saveModel.CONTENT = form["contenttext"];
            saveModel.PUB_DT_STR = form["publishDate"];
            saveModel.UPT_DT = DateTime.UtcNow.AddHours(8);
            saveModel.UPT_ID = UserProvider.Instance.User.ID;
            PublicMethodRepository.FilterXss(saveModel);

            if (ID == 0)
            {
                this.DB.NEWS.Add(saveModel);
            }
            else
            {
                this.DB.Entry(saveModel).State = EntityState.Modified;
            }

            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            int identityId = (int)saveModel.ID;

            return identityId;
        }

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<NEWS> data)
        {
            var r = data.Where(s => s.TITLE.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(string publishdate, ref List<NEWS> data)
        {
            var r = data.Where(s => s.PUB_DT_STR == publishdate).ToList();
            data = r;
        }

        /// <summary>
        /// 狀態搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListStatusFilter(string filter, string displayMode, ref List<NEWS> data)
        {
            List<NEWS> result = null;
            bool filterBooolean = Convert.ToBoolean(filter);
            if (displayMode == "DisplayHome")
            {
                result = data.Where(s => s.HOME_PAGE_DISPLAY == filterBooolean).ToList();
            }
            else if (displayMode == "Disable")
                result = data.Where(s => s.DISABLE == filterBooolean).ToList();
            data = result;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<NEWS> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, ref List<NEWS> data)
        {
            switch (sortCloumn)
            {
                case "sortPublishDate/asc":
                    data = data.OrderBy(o => o.PUB_DT_STR).ThenByDescending(o => o.SQ).ToList();
                    break;

                case "sortPublishDate/desc":
                    data = data.OrderByDescending(o => o.PUB_DT_STR).ThenByDescending(o => o.SQ).ToList();
                    break;

                case "sortIndex/asc":
                    data = data.OrderBy(o => o.SQ).ThenByDescending(g => g.PUB_DT_STR).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.SQ).ThenByDescending(g => g.PUB_DT_STR).ToList();
                    break;

                case "sortDisable/asc":
                    data = data.OrderBy(o => o.DISABLE).ThenByDescending(g => g.SQ).ToList();
                    break;

                case "sortDisable/desc":
                    data = data.OrderByDescending(o => o.DISABLE).ThenByDescending(g => g.SQ).ToList();
                    break;

                case "sortDisplayForHome/asc":
                    data = data.OrderBy(o => o.HOME_PAGE_DISPLAY).ThenByDescending(g => g.SQ).ToList();
                    break;

                case "sortDisplayForHome/desc":
                    data = data.OrderByDescending(o => o.HOME_PAGE_DISPLAY).ThenByDescending(g => g.SQ).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.PUB_DT_STR).ThenByDescending(g => g.SQ).ToList();
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