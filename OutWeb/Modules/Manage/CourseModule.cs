using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Models.Manage.ManageCourseModels;
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
    public class CourseModule : ListModuleService
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        public override void DoDeleteByID(int ID)
        {
            var data = this.DB.課程.Where(s => s.主索引 == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("[刪除國內外課程] 查無此課程，可能已被移除");
            try
            {
                var delFiles = this.DB.檔案.Where(o => o.對應名稱.StartsWith("Course") && o.對應索引 == ID).ToList();
                if (delFiles.Count > 0)
                {
                    foreach (var f in delFiles)
                        File.Delete(string.Concat(rootPath, f.檔案路徑));
                    this.DB.檔案.RemoveRange(delFiles);
                }
                this.DB.課程.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除國內外課程]" + ex.Message);
            }
        }

        public override object DoGetDetailsByID(int ID)
        {
            CourseDetailsDataModel result = new CourseDetailsDataModel();
            課程 data = DB.課程.Where(w => w.主索引 == ID).FirstOrDefault();
            PublicMethodRepository.HtmlDecode(data);
            result.Data = data;
            return result;
        }

        public override object DoGetList<TFilter>(TFilter model, Language language)
        {
            CourseListFilterModel filterModel = (model as CourseListFilterModel);
            PublicMethodRepository.FilterXss(filterModel);

            CourseListResultModel result = new CourseListResultModel();
            List<課程> data = new List<課程>();
            try
            {
                data = DB.課程.ToList();
                //語系搜尋
                //if (!language.Equals(Language.NotSet))
                //{
                //    this.CourseListFilterLanguage(language, ref data);
                //}
                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.CourseListFilter(filterModel.QueryString, ref data);
                }
                //發佈日期搜尋
                if (!string.IsNullOrEmpty(filterModel.PublishDate))
                {
                    this.CourseListDateFilter(Convert.ToDateTime(filterModel.PublishDate), ref data);
                }

                //前台顯示
                if (!string.IsNullOrEmpty(filterModel.DisplayForFrontEnd))
                {
                    this.CourseListStatusFilter(filterModel.DisplayForFrontEnd, "F", ref data);
                }

                //首頁顯示
                if (!string.IsNullOrEmpty(filterModel.DisplayForHomePage))
                {
                    this.CourseListStatusFilter(filterModel.DisplayForHomePage, "H", ref data);
                }

                //排序
                this.CourseListSort(filterModel.SortColumn, filterModel.Status, ref data);
                PaginationResult pagination;
                //分頁
                this.CourseListPageList(filterModel.CurrentPage, ref data, out pagination);
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
            課程 saveModel;
            FileRepository fileRepository = new FileRepository();
            if (!ID.HasValue)
            {
                saveModel = new 課程();
                saveModel.發稿人 = UserProvider.Instance.User.ID.ToString();
                saveModel.建立日期 = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.課程.Where(s => s.主索引 == ID).FirstOrDefault();
            }

            saveModel.標題 = form["標題"];
            saveModel.顯示狀態 = form["fSt"] == "Y" ? true : false;
            saveModel.首頁顯示 = form["hSt"] == null ? false : true;
            saveModel.排序 = form["sortIndex"] == null ? 1.0d : form["sortIndex"] == string.Empty ? 1.0d : Convert.ToDouble(form["sortIndex"]);
            saveModel.內容 = form["contenttext"];
            saveModel.發稿日期 = Convert.ToDateTime(form["發稿日期"]);
            saveModel.修改日期 = DateTime.UtcNow.AddHours(8);
            saveModel.修改人 = UserProvider.Instance.User.ID;
            saveModel.登錄人 = UserProvider.Instance.User.ID;
            saveModel.連結位址 = form["連結位址"];
            PublicMethodRepository.FilterXss(saveModel);
            if (ID.HasValue)
            {
                this.DB.Entry(saveModel).State = EntityState.Modified;
            }
            else
            {
                this.DB.課程.Add(saveModel);
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

            #region 若有null則是前端html的name重複於ajax formData名稱

            if (images != null)
            {
                if (images.Count > 0)
                    images.RemoveAll(item => item == null);
            }

            #endregion 若有null則是前端html的name重複於ajax formData名稱

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
                ActionName = "Course",
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
        private void CourseListFilter(string filterStr, ref List<課程> data)
        {
            var r = data.Where(s => s.標題.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void CourseListDateFilter(DateTime publishdate, ref List<課程> data)
        {
            var r = data.Where(s => s.發稿日期 == publishdate).ToList();
            data = r;
        }

        /// <summary>
        /// 前台顯示搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void CourseListStatusFilter(string status, string displayMode, ref List<課程> data)
        {
            List<課程> result = null;
            if (displayMode == "F")
            {
                if (status == "Y")
                    result = data.Where(s => s.顯示狀態 == true).ToList();
                else
                    result = data.Where(s => s.顯示狀態 == false).ToList();
            }
            else if (displayMode == "H")
                if (status == "Y")
                    result = data.Where(s => s.首頁顯示 == true).ToList();
                else
                    result = data.Where(s => s.首頁顯示 == false).ToList();
            data = result;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void CourseListPageList(int currentPage, ref List<課程> data, out PaginationResult pagination)
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
        private void CourseListSort(string sortCloumn, string status, ref List<課程> data)
        {
            switch (sortCloumn)
            {
                case "sortPublishDate/asc":
                    data = data.OrderBy(o => o.發稿日期).ToList();
                    break;

                case "sortPublishDate/desc":
                    data = data.OrderByDescending(o => o.發稿日期).ToList();
                    break;

                case "sortIndex/asc":
                    data = data.OrderBy(o => o.排序).ThenByDescending(g => g.發稿日期).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.排序).ThenByDescending(g => g.發稿日期).ToList();
                    break;

                case "sortDisplayForFront/asc":
                    data = data.OrderBy(o => o.顯示狀態).ThenByDescending(g => g.排序).ToList();
                    break;

                case "sortDisplayForFront/desc":
                    data = data.OrderByDescending(o => o.顯示狀態).ThenByDescending(g => g.排序).ToList();
                    break;

                case "sortDisplayForHome/asc":
                    data = data.OrderBy(o => o.首頁顯示).ThenByDescending(g => g.排序).ToList();
                    break;

                case "sortDisplayForHome/desc":
                    data = data.OrderByDescending(o => o.首頁顯示).ThenByDescending(g => g.排序).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.排序).ThenByDescending(g => g.發稿日期).ToList();
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