using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Models.Manage.ImgModels;
using OutWeb.Models.Manage.ManageLinkModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using OutWeb.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 最新消息列表模組
    /// </summary>
    public class LinkModule : ListModuleService
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        public override void DoDeleteByID(int ID)
        {
            var data = this.DB.外部連結.Where(s => s.主索引 == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("[刪除國內外外部連結] 查無此外部連結，可能已被移除");
            try
            {
                var delData = this.DB.檔案.Where(o => o.對應名稱.StartsWith("Link") && o.對應索引 == ID);
                var delFiles = this.DB.檔案.RemoveRange(delData);
                if (delData.ToList().Count > 0)
                {
                    foreach (var f in delFiles)
                        File.Delete(string.Concat(rootPath, f.檔案路徑));
                }
                this.DB.SaveChanges();

                this.DB.外部連結.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除外部連結]" + ex.Message);
            }
        }

        public override object DoGetDetailsByID(int ID)
        {
            LinkDetailsDataModel result = new LinkDetailsDataModel();
            外部連結 data = DB.外部連結.Where(w => w.主索引 == ID).FirstOrDefault();
            PublicMethodRepository.HtmlDecode(data);
            result.Data = data;
            return result;
        }

        public override object DoGetList<TFilter>(TFilter model, Language language)
        {
            LinkListFilterModel filterModel = (model as LinkListFilterModel);
            PublicMethodRepository.FilterXss(filterModel);
            LinkListResultModel result = new LinkListResultModel();
            List<外部連結> data = new List<外部連結>();
            try
            {
                data = DB.外部連結.ToList();

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }

                //狀態搜尋
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

        public override int DoSaveData(FormCollection form, Language language, int? ID = null, List<HttpPostedFileBase> images = null, List<HttpPostedFileBase> files = null)
        {
            外部連結 saveModel;
            ImageRepository imgepository = new ImageRepository();
            FileRepository fileRepository = new FileRepository();
            if (ID == 0)
            {
                saveModel = new 外部連結();
                saveModel.建立人員 = UserProvider.Instance.User.ID;
                saveModel.建立日期 = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.外部連結.Where(s => s.主索引 == ID).FirstOrDefault();
            }

            saveModel.名稱 = form["名稱"];
            saveModel.排序 = Convert.ToDouble(form["排序"]);
            saveModel.狀態 = form["狀態"] == "Y" ? true : false;
            saveModel.網址連結 = form["網址連結"];
            saveModel.更新人員 = UserProvider.Instance.User.ID;
            saveModel.更新日期 = DateTime.UtcNow.AddHours(8);
            PublicMethodRepository.FilterXss(saveModel);
            if (ID == 0)
            {
                this.DB.外部連結.Add(saveModel);
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
            int identityId = (int)saveModel.主索引;

            #region 圖片處理

            List<int> oldImgList = new List<int>();

            #region 將原存在的Server圖片保留 記錄圖片ID

            //將原存在的Server圖片保留 記錄圖片ID
            foreach (var f in form.Keys)
            {
                if (f.ToString().StartsWith("ImagesData"))
                {
                    var id = Convert.ToInt16(form[f.ToString().Split('.')[0] + ".ID"]);
                    if (!oldImgList.Contains(id))
                        oldImgList.Add(id);
                }
            }

            #endregion 將原存在的Server圖片保留 記錄圖片ID

            #region 建立圖片模型

            ImagesModel imgModel = new ImagesModel()
            {
                ActionName = "Links",
                ID = identityId,
                OldImageIds = oldImgList
            };

            #endregion 建立圖片模型

            #region 若有null則是前端html的name重複於ajax formData名稱

            if (images != null)
            {
                if (images.Count > 0)
                    images.RemoveAll(item => item == null);
            }

            #endregion 若有null則是前端html的name重複於ajax formData名稱

            #region img data binding 單筆多筆裝在不同容器

            //imgModel.UploadType = "images_m";
            imgepository.UploadPhoto("Post", imgModel, images, "M");
            imgepository.SaveImagesToDB(imgModel);

            #endregion img data binding 單筆多筆裝在不同容器

            #endregion 圖片處理

            #region 檔案處理

            List<int> oldFileList = new List<int>();

            #region 將原存在的Server檔案保留 記錄檔案ID

            //將原存在的Server檔案保留 記錄檔案ID
            foreach (var f in form.Keys)
            {
                if (f.ToString().StartsWith("FileData"))
                {
                    var id = Convert.ToInt16(form[f.ToString().Split('.')[0] + ".ID"]);
                    if (!oldFileList.Contains(id))
                        oldFileList.Add(id);
                }
            }

            #endregion 將原存在的Server檔案保留 記錄檔案ID

            #region 建立檔案模型

            FilesModel fileModel = new FilesModel()
            {
                ActionName = "Link",
                ID = identityId,
                OldFileIds = oldFileList
            };

            #endregion 建立檔案模型

            #region 若有null則是前端html的name重複於ajax formData名稱

            if (files != null)
            {
                if (files.Count > 0)
                    files.RemoveAll(item => item == null);
            }

            #endregion 若有null則是前端html的name重複於ajax formData名稱

            #region img data binding 單筆多筆裝在不同容器


            fileRepository.UploadFile("Post", fileModel, files, "M");
            fileRepository.SaveFileToDB(fileModel);

            #endregion img data binding 單筆多筆裝在不同容器

            #endregion 檔案處理

            return identityId;
        }

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<外部連結> data)
        {
            var r = data.Where(s => s.名稱.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 前台顯示搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListStatusFilter(string status, ref List<外部連結> data)
        {
            List<外部連結> result = null;
            result = data.Where(s => s.狀態 == (status.Equals("Y") ? true : false)).ToList();
            data = result;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<外部連結> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, string status, ref List<外部連結> data)
        {
            switch (sortCloumn)
            {
                case "sortIndex/asc":
                    data = data.OrderBy(o => o.排序).ThenByDescending(g => g.建立日期).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.排序).ThenByDescending(g => g.建立日期).ToList();
                    break;

                case "sortStatus/asc":
                    data = data.OrderBy(o => o.狀態).ThenByDescending(g => g.排序).ToList();
                    break;

                case "sortStatus/desc":
                    data = data.OrderByDescending(o => o.狀態).ThenByDescending(g => g.排序).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.排序).ThenByDescending(g => g.建立日期).ToList();
                    break;
            }
        }

        public override void Dispose()
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