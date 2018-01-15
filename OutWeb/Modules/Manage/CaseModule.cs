using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.CasesModels;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Models.Manage.ImgModels;
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
    public class CaseModule : ListModuleService
    {
        private DBEnergy m_DB = new DBEnergy();
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public override void DoDeleteByID(int ID)
        {
            var data = this.DB.能源案例.Where(s => s.主索引 == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("[刪除節能案例] 查無此新聞，可能已被移除");
            try
            {
                var delFiles = this.DB.檔案.Where(o => o.對應名稱.StartsWith("Case") && o.對應索引 == ID).ToList();
                if (delFiles.Count > 0)
                {
                    foreach (var f in delFiles)
                        File.Delete(string.Concat(rootPath, f.檔案路徑));
                    this.DB.檔案.RemoveRange(delFiles);
                }
                this.DB.能源案例.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除節能案例]" + ex.Message);
            }
        }

        public override object DoGetDetailsByID(int ID)
        {
            CasesDetailsDataModel result = new CasesDetailsDataModel();
            能源案例 data = DB.能源案例.Where(w => w.主索引 == ID).FirstOrDefault();
            PublicMethodRepository.HtmlDecode(data);
            result.Data = data;
            return result;
        }

        public override object DoGetList<TFilter>(TFilter model, Language language)
        {
            CasesListFilterModel filterModel = (model as CasesListFilterModel);
            PublicMethodRepository.FilterXss(filterModel);
            CasesListResultModel result = new CasesListResultModel();
            List<能源案例> data = new List<能源案例>();
            try
            {
                data = DB.能源案例.ToList();

                ////語系搜尋
                //if (!language.Equals(Language.NotSet))
                //{
                //    this.ListFilterLanguage(language, ref data);
                //}

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

                //顯示狀態
                if (!string.IsNullOrEmpty(filterModel.DisplayForFrontEnd))
                {
                    this.CourseListStatusFilter(filterModel.DisplayForFrontEnd, ref data);
                }

                //行業別搜尋
                //if (filterModel.TypeID2.HasValue)
                //{
                //    this.ListTypeFilter(filterModel.TypeID2.ToString(), ref data, 2);
                //}


                //前台顯示
                //if (!string.IsNullOrEmpty(filterModel.DisplayForFrontEnd))
                //{
                //    this.ListStatusFilter(filterModel.DisplayForFrontEnd, ref data);
                //}

                //前台顯示
                //this.ListSort(filterModel.SortColumn, filterModel.DisplayForFrontEnd, ref data);
                //排序
                this.ListSort(filterModel.SortColumn, filterModel.Status, ref data);
                PaginationResult pagination;
                //分頁
                this.ListPageList(filterModel.CurrentPage, ref data, out pagination);
                result.Pagination = pagination;
                foreach (var d in data)
                    PublicMethodRepository.FilterXss(d);
                result.Data = data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public override int DoSaveData(FormCollection form, Language language, int? ID = default(int?), List<HttpPostedFileBase> images = null, List<HttpPostedFileBase> files = null)
        {
            能源案例 saveModel;
            ImageRepository imgepository = new ImageRepository();
            FileRepository fileRepository = new FileRepository();

            if (!ID.HasValue)
            {
                saveModel = new 能源案例();
                saveModel.建立人員 = UserProvider.Instance.User.ID;
                saveModel.建立日期 = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.能源案例.Where(s => s.主索引 == ID).FirstOrDefault();
            }
            saveModel.發佈日期 = form["wkDt"] == null ? "" : form["wkDt"];
            saveModel.排序 = form["sortIndex"] == null ? 1 : form["sortIndex"] == string.Empty ? 1 : Convert.ToInt32(form["sortIndex"]);
            saveModel.案例標題 = form["案例標題"] == null ? "" : form["案例標題"];
            saveModel.內容 = form["contenttext"] == null ? "" : form["contenttext"];
            saveModel.顯示狀態 = form["fSt"] == "Y" ? true : false;
            saveModel.設備別 = form["type1"] == null ? 1 : form["type1"] == string.Empty ? 1 : Convert.ToInt32(form["type1"]);
            saveModel.行業別 = form["type2"] == null ? 1 : form["type2"] == string.Empty ? 1 : Convert.ToInt32(form["type2"]);
            saveModel.首頁顯示 = form["hSt"] == null ? false : true;
            saveModel.更新日期 = DateTime.UtcNow.AddHours(8);
            saveModel.更新人員 = UserProvider.Instance.User.ID;
            saveModel.樣板代碼 = Convert.ToInt16(form["tempID"]);
            PublicMethodRepository.FilterXss(saveModel);

            if (ID.HasValue)
                this.DB.Entry(saveModel).State = EntityState.Modified;
            else
                this.DB.能源案例.Add(saveModel);

            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            int identityId = saveModel.主索引;

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
                ActionName = "Cases",
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
                ActionName = "Cases",
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

            //fileModel.UploadType = "files_m";
            fileRepository.UploadFile("Post", fileModel, files, "M");
            fileRepository.SaveFileToDB(fileModel);

            #endregion img data binding 單筆多筆裝在不同容器

            #endregion 檔案處理

            return identityId;
        }

        /// <summary>
        /// 語系過濾條件
        /// </summary>
        /// <param name="language"></param>
        /// <param name="data"></param>
        //private void ListFilterLanguage(Language language, ref List<能源案例> data)
        //{
        //    var r = data.Where(s => s.Language == language.GetCode()).ToList();
        //    data = r;
        //}

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<能源案例> data)
        {
            var r = data.Where(s => s.案例標題.Contains(filterStr)).ToList();
            data = r;
        }


        /// <summary>
        /// 列表分類搜尋
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="data"></param>

        private void ListTypeFilter(string typeCode, ref List<能源案例> data, int typeKind)
        {
            if (typeKind == 1)
                data = data.Where(s => s.設備別.ToString().Contains(typeCode)).ToList();
            if (typeKind == 2)
                data = data.Where(s => s.行業別.ToString().Contains(typeCode)).ToList();
        }


        /// <summary>
        /// 前台顯示搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void CourseListStatusFilter(string status, ref List<能源案例> data)
        {
            List<能源案例> result = null;

            if (status == "Y")
                result = data.Where(s => s.顯示狀態 == true).ToList();
            else
                result = data.Where(s => s.顯示狀態 == false).ToList();
            data = result;
        }



        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(string publishdate, ref List<能源案例> data)
        {
            var r = data.Where(s => s.發佈日期 == publishdate).ToList();
            data = r;
        }

        /// <summary>
        /// 狀態搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        //private void ListStatusFilter(string filterStatus, ref List<能源案例> data)
        //{
        //    bool bDisplayForFront = filterStatus == "Y" ? true : false;
        //    var r = data.Where(s => s.DisplayForFront == bDisplayForFront).ToList();
        //    data = r;
        //}

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<能源案例> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, string status, ref List<能源案例> data)
        {
            switch (sortCloumn)
            {
                case "sortPublishDate/asc":
                    data = data.OrderBy(o => o.發佈日期).ToList();
                    break;

                case "sortPublishDate/desc":
                    data = data.OrderByDescending(o => o.發佈日期).ToList();
                    break;

                //case "sortDisplayForFront/asc":
                //    data = data.OrderBy(o => o.DisplayForFront).ThenByDescending(g => g.Sort).ToList();
                //    break;

                //case "sortDisplayForFront/desc":
                //    data = data.OrderByDescending(o => o.DisplayForFront).ThenByDescending(g => g.Sort).ToList();
                //    break;

                case "sortStutas/asc":
                    data = data.OrderBy(o => o.顯示狀態).ThenByDescending(g => g.發佈日期).ToList();
                    break;

                case "sortStutas/desc":
                    data = data.OrderByDescending(o => o.顯示狀態).ThenByDescending(g => g.發佈日期).ToList();
                    break;

                case "sortIndex/asc":
                    data = data.OrderBy(o => o.排序).ThenByDescending(g => g.發佈日期).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.排序).ThenByDescending(g => g.發佈日期).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.排序).ThenByDescending(g => g.發佈日期).ToList();
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