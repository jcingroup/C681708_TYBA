using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Models.Manage.ManageTrainModels;
using OutWeb.Modules.FrontEnd;
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
    public class TrainModule : ListModuleService
    {
        private DBEnergy m_DB = new DBEnergy();
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public override void DoDeleteByID(int ID)
        {
            var chk = DB.研討會報名對應檔.Where(o => o.對應研討會主索引 == ID).Count();

            if (chk > 0)
                throw new Exception("[刪除研討會] 已有人樹報名此課程，無法刪除。欲刪除請先取消報名。");
            try
            {
                var delFiles = this.DB.檔案.Where(o => o.對應名稱.StartsWith("Train") && o.對應索引 == ID).ToList();
                if (delFiles.Count > 0)
                {
                    foreach (var f in delFiles)
                        File.Delete(string.Concat(rootPath, f.檔案路徑));
                    this.DB.檔案.RemoveRange(delFiles);
                }
                DB.研討會明細檔.Remove(DB.研討會明細檔.Where(o => o.對應研討會主索引 == ID).First());
                DB.研討會主檔.Remove(DB.研討會主檔.Where(o => o.主索引 == ID).First());
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除研討會]" + ex.Message);
            }
        }

        public override object DoGetDetailsByID(int ID)
        {
            TrainDetailsDataModel result = new TrainDetailsDataModel();
            研討會主檔 data = DB.研討會主檔.Where(w => w.主索引 == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("無法取得研討會.");
            result.Data.ActivityName = data.研討會名稱;

            研討會明細檔 details = DB.研討會明細檔.Where(o => o.對應研討會主索引 == ID).FirstOrDefault();
            if (details == null)
                throw new Exception("無法取得研討會.");
            result.Data.ID = data.主索引;
            result.Data.ActivityName = data.研討會名稱;
            //result.Data.ActivityName = details.對應研討會主索引;
            result.Data.Organizer = details.主辦單位;
            result.Data.CoOrganiser = details.協辦單位;
            result.Data.DeadlineBegin = details.報名期限_起;
            result.Data.DeadlineEnd = details.報名期限_迄;
            result.Data.EnrollmentRestrictions = (int)details.活動人數上限;
            result.Data.SingleEnrollmentRestrictions = (int)details.用戶參加人數限制;
            result.Data.Sort = (int)details.排序;
            result.Data.ActivityContent = details.活動內容;
            result.Data.ActivityLocation = details.活動地點;
            result.Data.ActivityDateBegin = details.活動日期起 ?? DateTime.UtcNow.AddHours(8);
            result.Data.ActivityDateEnd = details.活動日期訖 ?? DateTime.UtcNow.AddHours(8);
            result.Data.ActivityTimeRange = details.活動時間範圍;
            result.Data.ContactPerson = details.聯絡人;
            result.Data.ContactPhoneNumber = details.連絡電話;
            result.Data.Remarks = details.備註;
            result.Data.ChargesStatus = details.收費狀態 ?? false;

            result.Data.ActivityStatus = (bool)details.顯示狀態;
            result.Data.DisplayHome = (bool)details.首頁顯示;
            PublicMethodRepository.HtmlDecode(result);
            PublicMethodRepository.HtmlDecode(result.Data);
            return result;
        }

        public override object DoGetList<TFilter>(TFilter model, Language language)
        {
            TrainListFilterModel filterModel = (model as TrainListFilterModel);
            PublicMethodRepository.FilterXss(filterModel);
            TrainListResultModel result = new TrainListResultModel();
            List<TrainStandardDataModel> data = new List<TrainStandardDataModel>();
            TrainFrontModule frontMdu = new TrainFrontModule();
            try
            {
                data =
                     this.DB.研討會主檔.Join(this.DB.研討會明細檔,
                       t1 => t1.主索引,
                       t2 => t2.對應研討會主索引,
                       (main, details) => new { Main = main, Details = details })
                       .ToList()
                       .Select(s => new TrainStandardDataModel()
                       {
                           ID = s.Main.主索引,
                           ActivityName = s.Main.研討會名稱,
                           CreateDate = s.Main.建立日期,
                           Organizer = s.Details.主辦單位,
                           CoOrganiser = s.Details.協辦單位,
                           DeadlineBegin = s.Details.報名期限_起,
                           DeadlineEnd = s.Details.報名期限_迄,
                           EnrollmentRestrictions = (int)s.Details.活動人數上限,
                           SingleEnrollmentRestrictions = (int)s.Details.用戶參加人數限制,
                           Sort = (int)s.Details.排序,
                           ActivityContent = s.Details.活動內容,
                           ActivityLocation = s.Details.活動地點,
                           ActivityDateBegin = s.Details.活動日期起 ?? DateTime.UtcNow.AddHours(8),
                           ActivityDateEnd = s.Details.活動日期訖 ?? DateTime.UtcNow.AddHours(8),
                           ActivityTimeRange = s.Details.活動時間範圍,
                           ActivityStatus = (bool)s.Details.顯示狀態,
                           ContactPerson = s.Details.聯絡人,
                           ContactPhoneNumber = s.Details.連絡電話,
                           Remarks = s.Details.備註,
                           DisplayHome = (bool)s.Details.首頁顯示,

                           RegistrationStatus = frontMdu.CheckRegistrationStatusByTrainID(s.Main.主索引)
                       })
                       .ToList();

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
                //發佈日期搜尋
                if (!string.IsNullOrEmpty(filterModel.PublishDate))
                {
                    this.ListDateFilter(Convert.ToDateTime(filterModel.PublishDate), ref data);
                }

                //顯示狀態
                if (!string.IsNullOrEmpty(filterModel.Status))
                {
                    this.ListStatusFilter(filterModel.Status, ref data, "0");
                }

                //首頁顯示狀態
                if (!string.IsNullOrEmpty(filterModel.HomeDisplay))
                {
                    this.ListStatusFilter(filterModel.HomeDisplay, ref data, "1");
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
            研討會主檔 saveMainModel;
            研討會明細檔 saveDetailsMainModel;
            FileRepository fileRepository = new FileRepository();
            if (!ID.HasValue)
            {
                saveMainModel = new 研討會主檔();
                saveMainModel.建立人員 = UserProvider.Instance.User.ID;
                saveMainModel.建立日期 = DateTime.UtcNow.AddHours(8);
                saveDetailsMainModel = new 研討會明細檔();
            }
            else
            {
                saveMainModel = this.DB.研討會主檔.Where(s => s.主索引 == ID).FirstOrDefault();
                saveDetailsMainModel = this.DB.研討會明細檔.Where(s => s.對應研討會主索引 == ID).FirstOrDefault();
            }
            saveMainModel.研討會名稱 = form["ActivityName"];
            saveMainModel.更新人員 = UserProvider.Instance.User.ID;
            saveMainModel.更新日期 = DateTime.UtcNow.AddHours(8);
            PublicMethodRepository.FilterXss(saveMainModel);
            if (ID.HasValue)
                this.DB.Entry(saveMainModel).State = EntityState.Modified;
            else
                this.DB.研討會主檔.Add(saveMainModel);
            this.DB.SaveChanges();

            saveDetailsMainModel.對應研討會主索引 = saveMainModel.主索引;
            saveDetailsMainModel.主辦單位 = form["Organizer"];
            saveDetailsMainModel.協辦單位 = form["CoOrganiser"];
            saveDetailsMainModel.報名期限_起 = form["DeadlineBegin"];
            saveDetailsMainModel.報名期限_迄 = form["DeadlineEnd"];
            saveDetailsMainModel.活動人數上限 = Convert.ToInt32(form["EnrollmentRestrictions"]);
            saveDetailsMainModel.用戶參加人數限制 = Convert.ToInt32(form["SingleEnrollmentRestrictions"]);
            saveDetailsMainModel.排序 = form["Sort"] == null ? 1.0d : form["Sort"] == string.Empty ? 1.0d : Convert.ToDouble(form["Sort"]);
            saveDetailsMainModel.活動內容 = form["ActivityContent"];
            saveDetailsMainModel.活動地點 = form["ActivityLocation"];
            saveDetailsMainModel.活動日期起 = Convert.ToDateTime(form["ActivityDateBegin"]);
            saveDetailsMainModel.活動日期訖 = Convert.ToDateTime(form["ActivityDateEnd"]);
            saveDetailsMainModel.活動時間範圍 = form["ActivityTimeRange"];
            saveDetailsMainModel.聯絡人 = form["ContactPerson"];
            saveDetailsMainModel.連絡電話 = form["ContactPhoneNumber"];
            saveDetailsMainModel.備註 = form["Remarks"];
            saveDetailsMainModel.顯示狀態 = Convert.ToBoolean(form["ActivityStatus"]);
            saveDetailsMainModel.收費狀態 = Convert.ToBoolean(form["ChargesStatus"]);

            //saveDetailsMainModel.報名狀態 = form["SignupStatus"] == null ? false : true;
            saveDetailsMainModel.顯示狀態 = form["ActivityStatus"] == "true" ? true : false;
            saveDetailsMainModel.首頁顯示 = form["DisplayHome"] == null ? false : true;
            //saveDetailsMainModel.前台顯示 = form["DisplayFront"] == null ? false : true;
            PublicMethodRepository.FilterXss(saveDetailsMainModel);

            if (ID.HasValue)
                this.DB.Entry(saveDetailsMainModel).State = EntityState.Modified;
            else
                this.DB.研討會明細檔.Add(saveDetailsMainModel);

            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            int identityId = (int)saveMainModel.主索引;

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
                ActionName = "Train",
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
        /// 報名人數統計
        /// </summary>
        /// <param name="trainID"></param>
        /// <returns></returns>
        private int MathAlreadyRegisteredCount(int trainID)
        {
            //研討會報名人員.對應報名檔主索引 => 研討會報名主檔.主索引
            //研討會報名主檔.主索引 =>研討會報名對應檔.對應研討會主索引
            int count = this.DB.研討會報名主檔
                                    .Join(
                                    this.DB.研討會報名對應檔,
                                    main => main.主索引,
                                    map => map.報名檔主索引,
                                    (main, map) => new { Main = main, Map = map })
                                    .Join(
                                    this.DB.研討會報名人員,
                                    mainData => mainData.Main.主索引,
                                    participate => participate.對應報名檔主索引,
                                    (data, participateData) => new { data, participateData })
                                    .Where(w => w.data.Map.對應研討會主索引 == trainID)
                                    .Select(s => s.participateData).Count();
            return count;
        }


        /// <summary>
        /// 列表分類搜尋
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="data"></param>

        //private void ListTypeFilter(string typeCode, ref List<新聞> data)
        //{
        //    data = data.Where(s => s.分類代碼.ToString().Contains(typeCode)).ToList();
        //}

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<TrainStandardDataModel> data)
        {
            var r = data.Where(s => s.ActivityName.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(DateTime date, ref List<TrainStandardDataModel> data)
        {
            var r = data.Where(s => s.CreateDate.Year == date.Year && s.CreateDate.Day == date.Day).ToList();
            data = r;
        }

        /// <summary>
        /// 前台顯示搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        /// <param name="mode">0=>顯示狀態 1=>首頁顯示</param>
        private void ListStatusFilter(string status, ref List<TrainStandardDataModel> data, string mode)
        {
            if (mode == "0")
            {
                if (status == "Y")
                    data = data.Where(s => s.ActivityStatus == true).ToList();
                else
                    data = data.Where(s => s.ActivityStatus == false).ToList();
            }
            else
            {
                if (status == "Y")
                    data = data.Where(s => s.DisplayHome == true).ToList();
                else
                    data = data.Where(s => s.DisplayHome == false).ToList();
            }
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<TrainStandardDataModel> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, string status, ref List<TrainStandardDataModel> data)
        {
            switch (sortCloumn)
            {
                case "sortStatus/asc":
                    data = data.OrderBy(o => o.ActivityStatus).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortStatus/desc":
                    data = data.OrderByDescending(o => o.ActivityStatus).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortDisplayForHome/asc":
                    data = data.OrderBy(o => o.DisplayHome).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortDisplayForHome/desc":
                    data = data.OrderByDescending(o => o.DisplayHome).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortIndex/asc":
                    data = data.OrderBy(o => o.Sort).ThenByDescending(g => g.ActivityDateBegin).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.Sort).ThenByDescending(g => g.ActivityDateBegin).ToList();
                    break;

                case "sortPublishDate/asc":
                    data = data.OrderBy(o => o.ActivityDateBegin).ThenByDescending(g => g.ActivityDateBegin).ToList();
                    break;

                case "sortPublishDate/desc":
                    data = data.OrderByDescending(o => o.ActivityDateBegin).ThenByDescending(g => g.ActivityDateBegin).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.Sort).ThenByDescending(o => o.ActivityDateBegin).ToList();
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