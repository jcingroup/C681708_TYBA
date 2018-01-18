using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.DownloadModels;
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
    public class DownloadModule : IDisposable
    {
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        public void DoDeleteByID(int ID)
        {
            var data = this.DB.DLFILES.Where(s => s.ID == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("[刪除檔案] 查無此檔案，可能已被移除");
            try
            {
                var delFiles = this.DB.FILEBASE.Where(o => o.MAP_SITE.StartsWith("Download") && o.MAP_ID == ID).ToList();
                if (delFiles.Count > 0)
                {
                    foreach (var f in delFiles)
                        File.Delete(string.Concat(rootPath, f.FILE_PATH));
                    this.DB.FILEBASE.RemoveRange(delFiles);
                }

                this.DB.DLFILES.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除檔案]" + ex.Message);
            }
        }

        public DownloadDetailsModel DoGetDetailsByID(int ID)
        {
            DownloadDetailsModel details =
                DB.DLFILES.Where(w => w.ID == ID)
                .Select(s => new DownloadDetailsModel()
                {
                    ID = s.ID,
                    Disable = s.DISABLE,
                    PublishDateStr = s.PUB_DT_STR,
                    Sort = s.SQ,
                    Title = s.TITLE
                })
                .FirstOrDefault();
            PublicMethodRepository.HtmlDecode(details);
            return details;
        }

        public DownloadListResultModel DoGetList(DownloadFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);
            DownloadListResultModel result = new DownloadListResultModel();
            List<DLFILES> data = new List<DLFILES>();
            try
            {
                data = DB.DLFILES.ToList();

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }
                //發佈日期搜尋
                if (!string.IsNullOrEmpty(filterModel.PublishDate))
                {
                    this.ListDateFilter((filterModel.PublishDate), ref data);
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

        public int DoSaveData(DownloadDataModel model)
        {
            DLFILES saveModel;
            ImageRepository imgepository = new ImageRepository();
            FileRepository fileRepository = new FileRepository();
            if (model.ID == 0)
            {
                saveModel = new DLFILES();
                saveModel.BUD_ID = UserProvider.Instance.User.ID;
                saveModel.BUD_DT = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.DLFILES.Where(s => s.ID == model.ID).FirstOrDefault();
            }
            saveModel.TITLE = model.Title;
            saveModel.SQ = model.Sort;
            saveModel.PUB_DT_STR = model.PublishDateStr;
            saveModel.DISABLE = model.Disable;
            saveModel.UPT_ID = UserProvider.Instance.User.ID;
            saveModel.UPT_DT = DateTime.UtcNow.AddHours(8);
            PublicMethodRepository.FilterXss(saveModel);

            if (model.ID == 0)
            {
                this.DB.DLFILES.Add(saveModel);
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
                ActionName = "Download",
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
        private void ListFilter(string filterStr, ref List<DLFILES> data)
        {
            var r = data.Where(s => s.TITLE.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(string publishdate, ref List<DLFILES> data)
        {
            var result = data.Where(s => s.PUB_DT_STR == publishdate).ToList();
            data = result;
        }

        /// <summary>
        /// 前台顯示搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListStatusFilter(string disable, ref List<DLFILES> data)
        {
            List<DLFILES> result = null;

            result = data.Where(s => s.DISABLE == Convert.ToBoolean(disable)).ToList();
            data = result;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<DLFILES> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, ref List<DLFILES> data)
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