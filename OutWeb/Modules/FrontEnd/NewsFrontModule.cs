using OutWeb.Entities;
using OutWeb.Models;
using OutWeb.Models.FrontEnd.NewsFrontEndModels;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OutWeb.Modules.FrontEnd
{
    public class NewsFrontModule : IDisposable
    {
        private DBEnergy DB = new DBEnergy();

        /// <summary>
        /// 取得出版品在分類管理中的代碼
        /// </summary>
        /// <returns></returns>
        private int GetNewsTypeID()
        {
            int newsTypeID = 0;
            try
            {
                newsTypeID =
                       this.DB.功能項目檔.Join(this.DB.分類對應檔,
                       t1 => t1.主索引,
                       t2 => t2.對應項目索引,
                       (item, map) => new { Item = item, Map = map })
                       .Where(o => o.Item.項目代碼 == "News")
                       .First().Map.對應分類類別索引;
            }
            catch
            {
                throw new Exception("無法取得新聞分類檔");
            }
            return newsTypeID;
        }
        public 分類明細檔 GetTypeByID(int ID)
        {
            var item = this.DB.功能項目檔.Where(o => o.項目代碼.StartsWith("News")).FirstOrDefault();
            if (item == null)
                throw new Exception("無法取得新聞分類");
            var map = this.DB.分類對應檔.Where(o => o.對應項目索引 == item.主索引).First();
            var type = this.DB.分類明細檔.Where(o => o.對應分類主檔索引 == map.對應分類類別索引).Where(o => o.主索引 == ID).FirstOrDefault();
            if (type == null)
                throw new Exception("尚未建立分類");
            return type;
        }

        public string GetNewsTypeNameByID(int typeID)
        {
            int bookTypeID = GetNewsTypeID();

            var type = this.DB.分類明細檔.Where(o => o.對應分類主檔索引 == bookTypeID && o.主索引 == typeID).FirstOrDefault();
            if (type == null)
                throw new Exception("無法取得出版品分類檔");
            PublicMethodRepository.HtmlDecode(new List<string> { type.分類名稱 });
            return type.分類名稱;
        }


        public 分類明細檔 GetDefualtType()
        {
            var item = this.DB.功能項目檔.Where(o => o.項目代碼.ToUpper().StartsWith("NEWS")).FirstOrDefault();
            if (item == null)
                throw new Exception("無法取得出版品分類");
            var map = this.DB.分類對應檔.Where(o => o.對應項目索引 == item.主索引).First();
            var type = this.DB.分類明細檔.Where(o => o.對應分類主檔索引 == map.對應分類類別索引).FirstOrDefault();
            if (type == null)
                throw new Exception("尚未建立分類");
            return type;
        }

     
        public NewsListFrontResultModel GetList(NewsListFrontFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);
            NewsListFrontResultModel result = new NewsListFrontResultModel();
            try
            {
                //var data = GetStandardNewsData();
                var data = this.DB.新聞
                                    .ToList()
                                    .Where(o => o.顯示狀態 == true)
                                    .Select(o => new NewsFrontListDataModel()
                                    {
                                        ID = o.主索引,
                                        Title = o.標題,
                                        Type = o.分類代碼,
                                        Status = o.顯示狀態,
                                        DisplayHome = o.首頁顯示,
                                        SortIndex = o.排序,
                                        PublishDate = o.發稿時間,
                                        NewsUrl = o.連結位址
                                    })
                                    .ToList();

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }
                //發佈日期搜尋
                this.ListDateFilter(filterModel.BeginDate, filterModel.EndDate, ref data);

                //分類
                if (!string.IsNullOrEmpty(filterModel.Type))
                {
                    this.ListTypeFilter(filterModel.Type, ref data);
                }

                //排序
                this.ListSort(filterModel.SortColumn, ref data);
                foreach (var d in data)
                    PublicMethodRepository.HtmlDecode(d);
                result.Data = data;

                //分頁
                result = this.ListPagination(ref result, filterModel.CurrentPage, Convert.ToInt32(PublicMethodRepository.GetConfigAppSetting("DefaultPageSize")));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 列表分類搜尋
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="data"></param>

        private void ListTypeFilter(string typeCode, ref List<NewsFrontListDataModel> data)
        {
            data = data.Where(s => s.Type.ToString().Contains(typeCode)).ToList();
        }

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<NewsFrontListDataModel> data)
        {
            var r = data.Where(s => s.Title.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(DateTime beginDate, DateTime endDate, ref List<NewsFrontListDataModel> data)
        {
            var r = data.Where(s => s.PublishDate >= beginDate && s.PublishDate <= endDate).ToList();
            data = r;
        }

        /// <summary>
        /// 列表排序功能
        /// </summary>
        /// <param name="sortCloumn"></param>
        /// <param name="data"></param>
        private void ListSort(string sortCloumn, ref List<NewsFrontListDataModel> data)
        {
            switch (sortCloumn)
            {
                default:
                    data = data.OrderByDescending(o => o.SortIndex).ThenBy(o => o.PublishDate).ToList();
                    break;
            }
        }

        /// <summary>
        /// [前台] 列表分頁處理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public NewsListFrontResultModel ListPagination(ref NewsListFrontResultModel model, int page, int pageSize)
        {
            int startRow = 0;
            PaginationResult paginationResult = null;
            if (pageSize > 0)
            {
                //分頁
                startRow = (page - 1) * pageSize;
                paginationResult = new PaginationResult()
                {
                    CurrentPage = page,
                    DataCount = model.Data.Count(),
                    PageSize = pageSize,
                    FirstPage = 1,
                    LastPage = model.Data.Count() == 0 ? 1 : Convert.ToInt32(Math.Ceiling((decimal)model.Data.Count() / pageSize))
                };
            }
            model.Data = model.Data.Skip(startRow).Take(pageSize).ToList();
            model.Pagination = paginationResult;
            return model;
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