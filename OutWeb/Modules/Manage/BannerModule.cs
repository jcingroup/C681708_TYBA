using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.BannerModels;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 最新消息列表模組
    /// </summary>
    public class BannerModule : IDisposable
    {
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        public void DoDeleteByID(int ID)
        {
            var data = this.DB.BANNER.Where(s => s.ID == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("[刪除檔案] 查無此檔案，可能已被移除");
            try
            {
                var delFiles = this.DB.FILEBASE.Where(o => o.MAP_SITE.StartsWith("Banner") && o.MAP_ID == ID).ToList();
                if (delFiles.Count > 0)
                {
                    foreach (var f in delFiles)
                        File.Delete(string.Concat(rootPath, f.FILE_PATH));
                    this.DB.FILEBASE.RemoveRange(delFiles);
                }

                this.DB.BANNER.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除檔案]" + ex.Message);
            }
        }

        public BannerDetailsModel DoGetDetailsByID(int ID)
        {
            BannerDetailsModel details =
                DB.BANNER.Where(w => w.ID == ID)
                .Select(s => new BannerDetailsModel()
                {
                    ID = s.ID,
                    Disable = s.DISABLE,
                    Sort = s.SQ,
                    Title = s.TITLE
                })
                .FirstOrDefault();
            PublicMethodRepository.HtmlDecode(details);
            return details;
        }

        public BannerListResultModel DoGetList(BannerFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);
            BannerListResultModel result = new BannerListResultModel();
            List<BANNER> data = new List<BANNER>();
            try
            {
                data = DB.BANNER.ToList();

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }

                //上下架
                if (!string.IsNullOrEmpty(filterModel.Disable))
                {
                    this.ListStatusFilter(filterModel.Disable, ref data);
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

        public int DoSaveData(BannerDataModel model)
        {
            BANNER saveModel;
            FileRepository fileRepository = new FileRepository();
            if (model.ID == 0)
            {
                saveModel = new BANNER();
                saveModel.BUD_ID = UserProvider.Instance.User.ID;
                saveModel.BUD_DT = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.BANNER.Where(s => s.ID == model.ID).FirstOrDefault();
            }
            saveModel.TITLE = model.Title;
            saveModel.SQ = model.Sort;
            saveModel.DISABLE = model.Disable;
            saveModel.UPT_ID = UserProvider.Instance.User.ID;
            saveModel.UPT_DT = DateTime.UtcNow.AddHours(8);
            PublicMethodRepository.FilterXss(saveModel);

            if (model.ID == 0)
            {
                this.DB.BANNER.Add(saveModel);
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

            #region 檔案處理


            FilesModel fileModel = new FilesModel()
            {
                ActionName = "Banner",
                ID = identityId,
                OldFileIds = model.OldFilesId
            };



            fileRepository.UploadFile("Post", fileModel, model.Files, "M");
            fileRepository.SaveFileToDB(fileModel);


            #endregion 檔案處理

            return identityId;
        }

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<BANNER> data)
        {
            var r = data.Where(s => s.TITLE.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 前台顯示搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListStatusFilter(string disable, ref List<BANNER> data)
        {
            List<BANNER> result = null;

            result = data.Where(s => s.DISABLE == Convert.ToBoolean(disable)).ToList();
            data = result;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<BANNER> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, ref List<BANNER> data)
        {
            switch (sortCloumn)
            {
                case "sortIndex/asc":
                    data = data.OrderBy(o => o.SQ).ThenByDescending(g => g.BUD_DT).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.SQ).ThenByDescending(g => g.BUD_DT).ToList();
                    break;

                case "sortDisable/asc":
                    data = data.OrderBy(o => o.DISABLE).ThenByDescending(g => g.SQ).ToList();
                    break;

                case "sortDisable/desc":
                    data = data.OrderByDescending(o => o.DISABLE).ThenByDescending(g => g.SQ).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.BUD_DT).ThenByDescending(g => g.SQ).ToList();
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