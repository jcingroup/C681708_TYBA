using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.ActivityModels;
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
    public class ActivityModule : IDisposable
    {
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        public void DoDeleteByID(int ID)
        {
            //檢查是否有人報名
            var valid = DB.APPLY.Where(o => o.MAP_ACT_ID == ID).ToList();
            if (valid.Count > 0)
                throw new Exception("[刪除檔案] 此紅動尚有隊伍報名中，無法移除");

            var data = this.DB.OLACT.Where(s => s.ID == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("[刪除檔案] 查無此活動，可能已被移除");
            try
            {
                var delFiles = this.DB.FILEBASE.Where(o => o.MAP_SITE.StartsWith("Activity") && o.MAP_ID == ID).ToList();
                if (delFiles.Count > 0)
                {
                    foreach (var f in delFiles)
                        File.Delete(string.Concat(rootPath, f.FILE_PATH));
                    this.DB.FILEBASE.RemoveRange(delFiles);
                }

                this.DB.OLACT.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除檔案]" + ex.Message);
            }
        }

        /// <summary>
        /// 驗證是否同組別裡有相同隊名 true：已有相同隊名 false：無相同隊名
        /// </summary>
        /// <param name="actId"></param>
        /// <param name="groupId"></param>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public bool ValidHasSeamTeamName(int actId, int groupId, string teamName)
        {
            var data = DB.APPLY.Where(o => o.MAP_ACT_ID == actId && o.MAP_ACT_GUP_ID == groupId && o.TEAM_NM == teamName.Trim()).ToList();
            return data.Count > 0;
        }

        public ActivityDetailsModel DoGetDetailsByID(int ID)
        {
            ActivityDetailsModel details =
                DB.OLACT.Where(w => w.ID == ID)
                .Select(s => new ActivityDetailsModel()
                {
                    ID = s.ID,
                    Disable = s.DISABLE,
                    PublishDateStr = s.PUB_DT_STR,
                    Sort = s.SQ,
                    Title = s.ACTITLE,
                    ActivityDateTimeDescription = s.ACT_DATE_DESC,
                    ActivityContent = s.ACT_CONTENT,
                    ApplyDateTimeBegin = s.APPLY_DATE_BEGIN,
                    ApplyDateTimeEnd = s.APPLY_DATE_END,
                    ActivityNumber = s.ACT_NUM
                })
                .FirstOrDefault();
            PublicMethodRepository.HtmlDecode(details);

            if (details != null)
            {
                details.ActivityGroup = DB.OLACTGROUP
                        .Where(o => o.MAP_ACT_ID == ID)
                        .Select(s => new ActivityGroup()
                        {
                            GroupID = s.ID,
                            GroupName = s.GROUP_NAME,
                            GroupApplyLimit = s.TEAM_APPLY_LIMIT,
                            CountApplyLimit = s.COUNT_APPLY_LIMIT
                        })
                        .ToList();
            }
            return details;
        }

        public ActivityListResultModel DoGetList(ActivityFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);
            ActivityListResultModel result = new ActivityListResultModel();
            Dictionary<int, bool> validActivityStatus = new Dictionary<int, bool>();
            List<OLACT> data = new List<OLACT>();
            try
            {
                data = DB.OLACT.ToList();

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
                foreach (var d in data)
                {
                    if (!validActivityStatus.ContainsKey(d.ID))
                        validActivityStatus.Add(d.ID, true);
                    validActivityStatus[d.ID] = ActivityStatusCheckByID(d.ID);
                }
                result.ValidActivityStatusBuffer = validActivityStatus;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public int DoSaveData(ActivityDataModel model)
        {
            OLACT saveModel;
            FileRepository fileRepository = new FileRepository();
            if (!model.ID.HasValue)
            {
                saveModel = new OLACT();
                saveModel.BUD_ID = UserProvider.Instance.User.ID;
                saveModel.BUD_DT = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.OLACT.Where(s => s.ID == model.ID).FirstOrDefault();
            }
            saveModel.ACTITLE = model.Title;
            saveModel.SQ = model.Sort;
            saveModel.PUB_DT_STR = model.PublishDateStr;
            saveModel.APPLY_DATE_BEGIN = model.ApplyDateTimeBegin;
            saveModel.APPLY_DATE_END = model.ApplyDateTimeEnd;
            saveModel.ACT_CONTENT = model.contenttext;
            saveModel.ACT_NUM = model.ActivityNumber;
            saveModel.ACT_DATE_DESC = model.ActivityDateTimeDescription;
            saveModel.DISABLE = model.Disable;
            saveModel.UPD_ID = UserProvider.Instance.User.ID;
            saveModel.UPD_DT = DateTime.UtcNow.AddHours(8);
            PublicMethodRepository.FilterXss(saveModel);

            if (!model.ID.HasValue)
            {
                this.DB.OLACT.Add(saveModel);
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

            #region 組別
            List<OLACTGROUP> groupUpdateEntity = new List<OLACTGROUP>();
            List<OLACTGROUP> groupAddEntity = new List<OLACTGROUP>();
            List<int> oldIds = model.ActivityGroup.Where(o => o.GroupID > 0).Select(s => s.GroupID).ToList();
            using (var transation = DB.Database.CurrentTransaction ?? DB.Database.BeginTransaction())
            {
                var removeNotInEntities = DB.OLACTGROUP.Where(o => o.MAP_ACT_ID == identityId && !oldIds.Contains(o.ID));
                if (removeNotInEntities.Count() > 0)
                    DB.OLACTGROUP.RemoveRange(removeNotInEntities);

                foreach (var group in model.ActivityGroup)
                {
                    OLACTGROUP temp = null;
                    if (group.GroupID == 0)
                    {
                        temp = new OLACTGROUP();

                        temp.BUD_DT = DateTime.UtcNow.AddHours(8);
                        temp.BUD_ID = UserProvider.Instance.User.ID;
                    }
                    else
                    {
                        temp = DB.OLACTGROUP.Where(o => o.ID == group.GroupID).First();
                    }
                    temp.MAP_ACT_ID = identityId;
                    temp.GROUP_NAME = group.GroupName;
                    temp.TEAM_APPLY_LIMIT = group.GroupApplyLimit;
                    temp.COUNT_APPLY_LIMIT = group.CountApplyLimit;
                    temp.UPD_DT = DateTime.UtcNow.AddHours(8);
                    temp.UPD_ID = UserProvider.Instance.User.ID;

                    if (group.GroupID == 0)
                    {
                        groupAddEntity.Add(temp);
                    }
                    else
                    {
                        groupUpdateEntity.Add(temp);
                    }
                }

                foreach (var item in groupUpdateEntity)
                {
                    this.DB.Entry(item).State = EntityState.Modified;
                }

                if (groupAddEntity.Count > 0)
                    this.DB.OLACTGROUP.AddRange(groupAddEntity);
                try
                {
                    DB.SaveChanges();
                    transation.Commit();
                }
                catch (Exception ex)
                {
                    transation.Rollback();
                    throw ex;
                }
            }
            #endregion


            #region 檔案處理

            FilesModel fileModel = new FilesModel()
            {
                ActionName = "Activity",
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
        private void ListFilter(string filterStr, ref List<OLACT> data)
        {
            var r = data.Where(s => s.ACTITLE.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(string publishdate, ref List<OLACT> data)
        {
            var result = data.Where(s => s.PUB_DT_STR == publishdate).ToList();
            data = result;
        }

        /// <summary>
        /// 上下架
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListStatusFilter(string disable, ref List<OLACT> data)
        {
            List<OLACT> result = null;

            result = data.Where(s => s.DISABLE == Convert.ToBoolean(disable)).ToList();
            data = result;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<OLACT> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, ref List<OLACT> data)
        {
            switch (sortCloumn)
            {
                case "sortApplyStatus/asc":
                    data = data.OrderBy(o => o.PUB_DT_STR).ThenByDescending(o => o.SQ).ToList();
                    break;

                case "sortApplyStatus/desc":
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


        public bool ActivityStatusCheckByID(int id)
        {
            bool valid = true;


            #region 驗證報名日期
            bool validActApplyDatetime = true;
            var act = DB.OLACT.Where(o => o.ID == id).FirstOrDefault();
            if (act == null)
                throw new Exception("無法取得該活動，是否已刪除?");
            //var actBeginTime = act.APPLY_DATE_BEGIN.ToDatetime().GetDateBeginTime();
            var actEndTime = act.APPLY_DATE_END.ToDatetime().GetDateEndTime();
            var today = DateTime.UtcNow.AddHours(8);
            var ticksTotalSeconds = new TimeSpan(today.Ticks - actEndTime.Ticks).TotalSeconds;
            if (ticksTotalSeconds >= 1)
                validActApplyDatetime = false;
            #endregion

            #region 驗證報名組數
            bool validApplyGroup = true;
            int groupLimitCount = DB.OLACTGROUP.Where(o => o.MAP_ACT_ID == id).Sum(s => s.TEAM_APPLY_LIMIT);
            int currentApplyGroup = DB.APPLY.Where(g => g.MAP_ACT_ID == id).Count();
            validApplyGroup = (groupLimitCount > currentApplyGroup);

            #endregion


            if (!validActApplyDatetime || !validApplyGroup)
            {
                valid = false;
            }

            return valid;


        }

    }
}