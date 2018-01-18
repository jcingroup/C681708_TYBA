using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Models.Manage.ImgModels;
using OutWeb.Models.Manage.ManageBookModels;
using OutWeb.Provider;
using OutWeb.Repositories;
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
    public class BookModule : IDisposable
    {
        private DBEnergy m_DB = new DBEnergy();
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public void DoDeleteByID(int ID)
        {
            var data = this.DB.出版品主檔.Where(s => s.主索引 == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("[刪除國內外出版品主檔] 查無此出版品主檔，可能已被移除");
            try
            {
                var delFiles = this.DB.檔案.Where(o => o.對應名稱.StartsWith("Book") && o.對應索引 == ID).ToList();
                if (delFiles.Count > 0)
                {
                    foreach (var f in delFiles)
                        File.Delete(string.Concat(rootPath, f.檔案路徑));
                    this.DB.檔案.RemoveRange(delFiles);
                }
                this.DB.出版品主檔.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除國內外出版品主檔]" + ex.Message);
            }
        }

        public BookDetailsDataModel DoGetDetailsByID(int ID)
        {
            BookDetailsDataModel result = new BookDetailsDataModel();
            出版品主檔 data = DB.出版品主檔.Where(w => w.主索引 == ID).FirstOrDefault();
            PublicMethodRepository.HtmlDecode(data);
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 取得出版品在分類管理中的代碼
        /// </summary>
        /// <returns></returns>
        public int GetBookTypeID()
        {
            int bookTypeID = 0;
            try
            {
                bookTypeID =
                       this.DB.功能項目檔.Join(this.DB.分類對應檔,
                       t1 => t1.主索引,
                       t2 => t2.對應項目索引,
                       (item, map) => new { Item = item, Map = map })
                       .Where(o => o.Item.項目代碼 == "Book")
                       .First().Map.對應分類類別索引;
            }
            catch
            {
                throw new Exception("無法取得出版品分類檔");
            }
            return bookTypeID;
        }

        /// <summary>
        /// 取得分類名稱
        /// </summary>
        /// <param name="typeID"></param>
        /// <returns></returns>
        private string GetBookTypeNameByID(int typeID)
        {
            int bookTypeID = GetBookTypeID();

            var type = this.DB.分類明細檔.Where(o => o.對應分類主檔索引 == bookTypeID && o.主索引 == typeID).FirstOrDefault();
            if (type == null)
                throw new Exception("無法取得出版品分類檔");
            PublicMethodRepository.HtmlDecode(new List<string> { type.分類名稱 });
            return type.分類名稱;
        }

        public BookListResultModel DoGetList(BookListFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);

            BookListResultModel result = new BookListResultModel();
            List<BookListDataModel> data = new List<BookListDataModel>();
            try
            {
                data = DB.出版品主檔
                    .ToList()
                    .Select(o => new BookListDataModel()
                    {
                        ID = o.主索引,
                        Title = o.名稱,
                        Remark = o.摘要,
                        CreateDate = o.建立時間,
                        PublishDate = o.發稿時間,
                        Sort = o.排序,
                        Status = o.顯示狀態,
                        TypeID = o.對應分類索引,
                        TypeName = GetBookTypeNameByID(o.對應分類索引)
                    })
                    .ToList();

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }
                //發佈日期搜尋
                this.ListDateFilter(filterModel.PublishBeginDate, filterModel.PublishEndDate, ref data);

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

        public int DoSaveData(FormCollection form,
            int? ID = null,
            List<HttpPostedFileBase> cover = null,
            List<HttpPostedFileBase> full = null,
            List<HttpPostedFileBase> chapter = null)
        {
            出版品主檔 saveModel;
            ImageRepository imgepository = new ImageRepository();
            FileRepository fileRepository = new FileRepository();
            if (!ID.HasValue)
            {
                saveModel = new 出版品主檔();
                saveModel.發稿人 = UserProvider.Instance.User.ID.ToString();
                saveModel.建立時間 = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.出版品主檔.Where(s => s.主索引 == ID).FirstOrDefault();
            }

            saveModel.名稱 = form["name"];
            saveModel.對應分類索引 = Convert.ToInt16(form["type"]);
            saveModel.排序 = form["sortIndex"] == null ? 1.0d : form["sortIndex"] == string.Empty ? 1.0d : Convert.ToDouble(form["sortIndex"]);
            saveModel.摘要 = form["summary"];
            saveModel.顯示狀態 = form["status"] == "false" ? false : true;
            saveModel.發稿時間 = Convert.ToDateTime(form["publishDate"]);
            saveModel.更新人員 = UserProvider.Instance.User.ID;
            saveModel.更新時間 = DateTime.UtcNow.AddHours(8);

            PublicMethodRepository.FilterXss(saveModel);
            if (ID.HasValue)
            {
                this.DB.Entry(saveModel).State = EntityState.Modified;
            }
            else
            {
                this.DB.出版品主檔.Add(saveModel);
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
                ActionName = "Book",
                ID = identityId,
                OldImageIds = oldImgList
            };

            #endregion 建立圖片模型

            #region 若有null則是前端html的name重複於ajax formData名稱

            if (cover != null)
            {
                if (cover.Count > 0)
                    cover.RemoveAll(item => item == null);
            }

            #endregion 若有null則是前端html的name重複於ajax formData名稱

            #region img data binding 單筆多筆裝在不同容器

            //imgModel.UploadType = "images_m";
            imgepository.UploadPhoto("Post", imgModel, cover, "M");
            imgepository.SaveImagesToDB(imgModel);

            #endregion img data binding 單筆多筆裝在不同容器

            #endregion 圖片處理

            #region 檔案處理 全文

            List<int> oldFullFileList = new List<int>();

            #region 將原存在的Server檔案保留 記錄檔案ID

            //將原存在的Server檔案保留 記錄檔案ID
            foreach (var f in form.Keys)
            {
                if (f.ToString().StartsWith("FullBookFile"))
                {
                    var id = Convert.ToInt16(form[f.ToString().Split('.')[0] + ".ID"]);
                    if (!oldFullFileList.Contains(id))
                        oldFullFileList.Add(id);
                }
            }

            #endregion 將原存在的Server檔案保留 記錄檔案ID

            #region 建立檔案模型

            FilesModel fileModel = new FilesModel()
            {
                ActionName = "Book",
                ID = identityId,
                OldFileIds = oldFullFileList,
                UploadIdentify = FileUploadIdentifyType.MODE1
            };

            #endregion 建立檔案模型

            #region 若有null則是前端html的name重複於ajax formData名稱

            if (full != null)
            {
                if (full.Count > 0)
                    full.RemoveAll(item => item == null);
            }

            #endregion 若有null則是前端html的name重複於ajax formData名稱

            #region img data binding 單筆多筆裝在不同容器

            //fileModel.UploadType = "files_m";
            fileRepository.UploadFile("Post", fileModel, full, "M");
            fileRepository.SaveFileToDB(fileModel);

            #endregion img data binding 單筆多筆裝在不同容器

            #endregion 檔案處理 全文

            #region 檔案處理 章節

            List<int> oldChapterFileList = new List<int>();

            #region 將原存在的Server檔案保留 記錄檔案ID

            //將原存在的Server檔案保留 記錄檔案ID
            foreach (var f in form.Keys)
            {
                if (f.ToString().StartsWith("ChapterFiles"))
                {
                    var id = Convert.ToInt16(form[f.ToString().Split('.')[0] + ".ID"]);
                    if (!oldChapterFileList.Contains(id))
                        oldChapterFileList.Add(id);
                }
            }

            #endregion 將原存在的Server檔案保留 記錄檔案ID

            #region 建立檔案模型 章節

            FilesModel fileChapterModel = new FilesModel()
            {
                ActionName = "Book",
                ID = identityId,
                OldFileIds = oldChapterFileList,
                UploadIdentify = FileUploadIdentifyType.MODE2
            };

            #endregion 建立檔案模型 章節

            #region 若有null則是前端html的name重複於ajax formData名稱

            if (full != null)
            {
                if (full.Count > 0)
                    full.RemoveAll(item => item == null);
            }

            #endregion 若有null則是前端html的name重複於ajax formData名稱

            #region img data binding 單筆多筆裝在不同容器

            //fileModel.UploadType = "files_m";
            fileRepository.UploadFile("Post", fileChapterModel, chapter, "M");
            fileRepository.SaveFileToDB(fileChapterModel);

            #endregion img data binding 單筆多筆裝在不同容器

            #region 章節明細

            List<BookChapterDetalisModel> chapterDetailsFilter = new List<BookChapterDetalisModel>();
            List<BookChapterDetalisModel> chapterDetails = new List<BookChapterDetalisModel>();
            var findDetails = form.AllKeys.Where(o => o.StartsWith("chapterDetails"))
            .ToDictionary(k => k, k => form[k]);

            if (findDetails.Count == 0)
            {
                this.DB.出版品章節明細檔.RemoveRange(DB.出版品章節明細檔.Where(o => o.對應出版品索引 == identityId).ToList());
                this.DB.SaveChanges();
                return identityId;
            }

            foreach (var d in findDetails)
            {
                string findIndex = d.Key.ToString().Substring(d.Key.ToString().IndexOf("[") + 1, d.Key.ToString().IndexOf("]") - d.Key.ToString().IndexOf("[") - 1);
                BookChapterDetalisModel temp = new BookChapterDetalisModel()
                {
                    ID = Convert.ToInt16(string.IsNullOrEmpty(findDetails["chapterDetails[" + findIndex + "].ID"]) ? Convert.ToInt32(findIndex) :
                            Convert.ToInt16(findDetails["chapterDetails[" + findIndex + "].ID"])),
                    MapFileID = Convert.ToInt16(string.IsNullOrEmpty(findDetails["chapterDetails[" + findIndex + "].MapFileID"]) ? 0 :
                            Convert.ToInt16(findDetails["chapterDetails[" + findIndex + "].MapFileID"])),
                    Alias = Convert.ToString(findDetails["chapterDetails[" + findIndex + "].Alias"]),
                    SQ = Convert.ToInt16(findDetails["chapterDetails[" + findIndex + "].SQ"]),
                };
                chapterDetailsFilter.Add(temp);
            }

            if (chapterDetailsFilter.Count > 0)
            {
                IEnumerable<IGrouping<int, BookChapterDetalisModel>> query =
                                         chapterDetailsFilter.GroupBy(x => (int)x.ID);
                foreach (IGrouping<int, BookChapterDetalisModel> group in query)
                {
                    chapterDetails.Add(new BookChapterDetalisModel()
                    {
                        ID = group.First().ID,
                        Alias = group.First().Alias,
                        MapFileID = group.First().MapFileID,
                        SQ = group.First().SQ
                    });
                }
            }

            if (!ID.HasValue)
            {
                FileModule fileModule = new FileModule();
                var orderbyData = fileModule.GetFiles(identityId, "Book", "M", FileUploadIdentifyType.MODE2);
                orderbyData = orderbyData.OrderBy(o => o.ID).ToList();
                List<出版品章節明細檔> fileDetails = new List<出版品章節明細檔>();
                foreach (var cha in orderbyData)
                {
                    int index = orderbyData.IndexOf(cha);
                    出版品章節明細檔 temp = new 出版品章節明細檔()
                    {
                        對應出版品索引 = identityId,
                        對應檔案索引 = (int)cha.ID,
                        檔案別名 = chapterDetails[index].Alias,
                        排序 = chapterDetails[index].SQ
                    };
                    fileDetails.Add(temp);
                }
                foreach (var f in fileDetails)
                    PublicMethodRepository.FilterXss(f);
                this.DB.出版品章節明細檔.AddRange(fileDetails);
                this.DB.SaveChanges();
            }
            else
            {
                if (chapterDetails.Count > 0)
                {
                    if (oldChapterFileList.Count > 0)
                    {
                        this.DB.出版品章節明細檔.RemoveRange(DB.出版品章節明細檔.Where(o => !oldChapterFileList.Contains(o.對應檔案索引)
                                  && o.對應出版品索引 == identityId
                                  ));
                        this.DB.SaveChanges();
                    }

                    foreach (var cha in chapterDetails)
                    {
                        if (cha.MapFileID > 0)
                        {
                            //var details = chapterDetails.Where(o => o.MapFileID == cha).First();
                            var entity = this.DB.出版品章節明細檔.Where(o => o.對應出版品索引 == identityId && o.主索引 == cha.ID).First();
                            entity.檔案別名 = cha.Alias;
                            entity.排序 = cha.SQ;
                            PublicMethodRepository.FilterXss(entity);
                            this.DB.Entry(entity).State = EntityState.Modified;
                            this.DB.SaveChanges();
                        }
                        else
                        {
                            FileModule fileModule = new FileModule();
                            var orderbyData = fileModule.GetFiles(identityId, "Book", "M", FileUploadIdentifyType.MODE2);
                            orderbyData = orderbyData.Where(o => !oldChapterFileList.Contains((int)o.ID)).OrderBy(o => o.ID).ToList();
                            if (orderbyData.Count > 0)
                            {
                                出版品章節明細檔 temp = new 出版品章節明細檔()
                                {
                                    對應出版品索引 = identityId,
                                    對應檔案索引 = (int)orderbyData.First().ID,
                                    排序 = cha.SQ,
                                    檔案別名 = cha.Alias
                                };
                                PublicMethodRepository.FilterXss(temp);
                                this.DB.出版品章節明細檔.Add(temp);
                                this.DB.SaveChanges();
                                oldChapterFileList.Add((int)orderbyData.First().ID);
                            }
                        }
                    }
                }
            }

            #endregion 章節明細

            #endregion 檔案處理 章節

            return identityId;
        }

        public BookChapterDetalisModel GetChapterDetailsByFileID(int bookID, int fileID)
        {
            BookChapterDetalisModel details = this.DB.出版品章節明細檔
                .Where(o => o.對應出版品索引 == bookID && o.對應檔案索引 == fileID)
                .Select(s => new BookChapterDetalisModel()
                {
                    ID = s.主索引,
                    Alias = s.檔案別名,
                    SQ = (float)s.排序,
                    MapFileID = s.對應檔案索引
                })
                .FirstOrDefault();

            PublicMethodRepository.HtmlDecode(details);
            return details;
        }

        /// <summary>
        /// 列表分類搜尋
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="data"></param>

        private void ListTypeFilter(string typeCode, ref List<BookListDataModel> data)
        {
            data = data.Where(s => s.TypeID.ToString().Contains(typeCode)).ToList();
        }

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<BookListDataModel> data)
        {
            var r = data.Where(s => s.Title.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(DateTime publishBegindate, DateTime publishEnddate, ref List<BookListDataModel> data)
        {
            var r = data.Where(s => s.PublishDate >= publishBegindate && s.PublishDate <= publishEnddate).ToList();
            data = r;
        }

        /// <summary>
        /// 顯狀態示搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListStatusFilter(string status, ref List<BookListDataModel> data)
        {
            List<BookListDataModel> result = null;

            if (status == "Y")
                result = data.Where(s => s.Status == true).ToList();
            else
                result = data.Where(s => s.Status == false).ToList();

            data = result;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<BookListDataModel> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, string status, ref List<BookListDataModel> data)
        {
            switch (sortCloumn)
            {
                case "sortPublishDate/asc":
                    data = data.OrderBy(o => o.PublishDate).ToList();
                    break;

                case "sortPublishDate/desc":
                    data = data.OrderByDescending(o => o.PublishDate).ToList();
                    break;

                case "sortIndex/asc":
                    data = data.OrderBy(o => o.Sort).ThenByDescending(g => g.PublishDate).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.Sort).ThenByDescending(g => g.PublishDate).ToList();
                    break;

                case "sortStatus/asc":
                    data = data.OrderBy(o => o.Status).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortStatus/desc":
                    data = data.OrderByDescending(o => o.Status).ThenByDescending(g => g.Sort).ToList();
                    break;
                case "sortType/asc":
                    data = data.OrderBy(o => o.TypeID).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortType/desc":
                    data = data.OrderByDescending(o => o.TypeID).ThenByDescending(g => g.Sort).ToList();
                    break;
                default:
                    data = data.OrderByDescending(o => o.Sort).ThenByDescending(g => g.PublishDate).ToList();
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