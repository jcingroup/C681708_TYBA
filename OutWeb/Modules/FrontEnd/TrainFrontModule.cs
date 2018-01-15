using OutWeb.Entities;
using OutWeb.Models;
using OutWeb.Models.FrontEnd.TrainModels.TrainApplyModels;
using OutWeb.Models.FrontEnd.TrainModels.TrainListModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OutWeb.Modules.FrontEnd
{
    public class TrainFrontModule : IDisposable
    {
        private DBEnergy DB = new DBEnergy();

        /// <summary>
        ///
        /// </summary>
        /// <param name="trainID"></param>
        /// <returns></returns>
        public int GetApplyByUserAccountAndTrainID(int trainID)
        {
            var apply = this.DB.研討會報名對應檔.Where(o => o.對應研討會主索引 == trainID
            && o.報名用戶編號 == UserFrontProvider.Instance.User.UserAccount).FirstOrDefault();
            if (apply == null)
                throw new Exception("用戶未報名過此課程");
            int applyID = apply.報名檔主索引;
            return applyID;
        }

        /// <summary>
        /// 依用戶編號判斷是否已報名過課程 return bool true=>已報名
        /// </summary>
        /// <param name="trainID"></param>
        /// <returns></returns>
        public void CheckHasEnrollTrain(int trainID)
        {
            var chk = this.DB.研討會報名對應檔.Where(o => o.對應研討會主索引 == trainID && o.報名用戶編號 == UserFrontProvider.Instance.User.UserAccount).FirstOrDefault();
            if (chk != null)
                throw new Exception("已報名過該課程，請勿重複報名");
        }

        /// <summary>
        /// 報名人數統計
        /// </summary>
        /// <param name="trainID"></param>
        /// <returns></returns>
        private int MathAlreadyRegisteredCount(int trainID, int? applyID = null)
        {
            //研討會報名人員.對應報名檔主索引 => 研討會報名主檔.主索引
            //研討會報名主檔.主索引 =>研討會報名對應檔.對應研討會主索引

            int count = 0;
            if (applyID == null)
            {
                count = this.DB.研討會報名主檔
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
            }
            else
            {
                count = this.DB.研討會報名主檔
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
                                 .Where(w => w.data.Map.對應研討會主索引 == trainID
                                 && w.data.Map.報名檔主索引 == (int)applyID)
                                 .Select(s => s.participateData).Count();
            }

            return count;
        }

        /// <summary>
        /// 報名人數狀態統計 0=>取消報名 1=>完成報名 2=>額滿候補
        /// </summary>
        /// <param name="trainID"></param>
        /// <returns></returns>
        private int MathApplyStatusCount(int trainID, int applyID, int statusNumber)
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
                                    .Where(w => w.data.Map.對應研討會主索引 == trainID
                                    && w.data.Map.報名檔主索引 == applyID
                                    && w.participateData.報名狀態 == statusNumber)
                                    .Select(s => s.participateData).Count();
            return count;
        }

        public TrainListResultModel GetList(TrainListFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);
            TrainListResultModel result = new TrainListResultModel();
            try
            {
                //var data = GetStandardTrainData();
                var data = this.DB.研討會主檔
                                .Join(
                                     this.DB.研討會明細檔,
                                    main => main.主索引,
                                    map => map.對應研討會主索引,
                                    (main, details) => new { Main = main, Details = details })
                                    .Where(o => (bool)o.Details.顯示狀態)
                                    .ToList()
                                    .Select(o => new TrainListDataModel()
                                    {
                                        ID = o.Main.主索引,
                                        ActivityDateBegin = (DateTime)o.Details.活動日期起,
                                        ActivityDateEnd = (DateTime)o.Details.活動日期訖,
                                        ActivityDateStr = string.Format("{0}~{1}", o.Details.活動日期起.Value.ToString("yyyy\\/MM\\/dd"), o.Details.活動日期訖.Value.ToString("yyyy\\/MM\\/dd")),
                                        ActivityName = o.Main.研討會名稱,
                                        ActivityStatus = (bool)o.Details.顯示狀態,
                                        AlreadyRegistered = MathAlreadyRegisteredCount(o.Main.主索引),
                                        LimitCount = (int)o.Details.活動人數上限,
                                        RegistrationBeginDate = Convert.ToDateTime(o.Details.報名期限_起),
                                        RegistrationEndDate = Convert.ToDateTime(o.Details.報名期限_迄).AddHours(23).AddMinutes(59),
                                        Sort = (double)o.Details.排序
                                    })
                                    .ToList();
                foreach (var ap in data)
                {
                    bool chk = CheckRegistrationStatusByTrainID(ap.ID);
                    ap.RegistrationStatusDescription = chk ? "報名" : "報名截止";
                    ap.IsStopRegistering = chk;
                }
                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }
                //發佈日期搜尋
                this.ListDateFilter(filterModel.BeginDate, filterModel.EndDate, ref data);

                ////排序
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

        public TrainListResultModel GetListByUser(TrainListFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);

            TrainListResultModel result = new TrainListResultModel();
            try
            {
                //var data = GetStandardTrainData();
                //if (data.Count > 0)
                //    data = data.Where(o => o.MapUserAccount == UserFrontProvider.Instance.User.UserAccount).ToList();
                var data = this.DB.研討會主檔
                                .Join(
                                     this.DB.研討會明細檔,
                                    main => main.主索引,
                                    map => map.對應研討會主索引,
                                    (main, details) => new { Main = main, Details = details })
                                    .Join(
                                     this.DB.研討會報名對應檔,
                                    M => M.Main.主索引,
                                    D => D.對應研討會主索引,
                                    (main, details) => new { M = main, D = details })
                                    .Where(o => o.D.報名用戶編號 == UserFrontProvider.Instance.User.UserAccount)
                                    .ToList()
                                    .Select(o => new TrainListDataModel()
                                    {
                                        ID = o.M.Main.主索引,
                                        ApplyID = o.D.報名檔主索引,
                                        ActivityName = o.M.Main.研討會名稱,
                                        ActivityDateBegin = (DateTime)o.M.Details.活動日期起,
                                        ActivityDateEnd = (DateTime)o.M.Details.活動日期訖,
                                        ActivityDateStr = string.Format("{0}~{1}", o.M.Details.活動日期起.Value.ToString("yyyy\\/MM\\/dd"), o.M.Details.活動日期訖.Value.ToString("yyyy\\/MM\\/dd")),
                                        ActivityStatus = (bool)o.M.Details.顯示狀態,
                                        AlreadyRegistered = MathAlreadyRegisteredCount(o.M.Main.主索引, o.D.報名檔主索引),
                                        Completed = MathApplyStatusCount(o.M.Main.主索引, o.D.報名檔主索引, 1),
                                        FullOfWaiting = MathApplyStatusCount(o.M.Main.主索引, o.D.報名檔主索引, 2),
                                        LimitCount = (int)o.M.Details.活動人數上限,
                                        ChargesStatus = o.M.Details.收費狀態 ?? false
                                    })
                                    .ToList();

                foreach (var ap in data)
                {
                    PublicMethodRepository.HtmlDecode(ap);
                    bool chk = CheckRegistrationStatusByTrainID(ap.ID);
                    ap.RegistrationStatusDescription = chk ? "報名" : "報名截止";
                    ap.IsStopRegistering = chk;
                }
                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }
                //發佈日期搜尋
                this.ListDateFilter(filterModel.BeginDate, filterModel.EndDate, ref data);

                ////排序
                this.ListSort(filterModel.SortColumn, ref data);

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
        /// 判斷是否可以報名的條件
        /// </summary>
        /// <param name="trainID"></param>
        /// <returns></returns>
        public bool CheckRegistrationStatusByTrainID(int trainID)
        {
            var apply = this.DB.研討會主檔
                              .Join(
                                   this.DB.研討會明細檔,
                                  main => main.主索引,
                                  map => map.對應研討會主索引,
                                  (main, details) => new { Main = main, Details = details })
                                  .Where(o => o.Main.主索引 == trainID)
                                  .ToList()
                                  .Select(o => new TrainListDataModel()
                                  {
                                      ID = o.Main.主索引,
                                      ActivityName = o.Main.研討會名稱,
                                      ActivityDateStr = o.Main.建立日期.ToString("yyyy\\/MM\\/dd"),
                                      ActivityStatus = true,
                                      AlreadyRegistered = MathAlreadyRegisteredCount(o.Main.主索引),
                                      LimitCount = (int)o.Details.活動人數上限,
                                      RegistrationBeginDate = Convert.ToDateTime(o.Details.報名期限_起),
                                      RegistrationEndDate = Convert.ToDateTime(o.Details.報名期限_迄).AddHours(23).AddMinutes(59),
                                      Sort = (double)o.Details.排序
                                  })
                                  .FirstOrDefault();

            if (apply == null)
            {
                throw new Exception("無法取得研討會");
            }
            var today = DateTime.UtcNow.AddHours(8);
            var gebeginDateTick = apply.RegistrationBeginDate.Ticks;
            var endDateTick = apply.RegistrationEndDate.Ticks;
            var chkDate = ((new TimeSpan(gebeginDateTick - today.Ticks).TotalSeconds <= 1) &&
                (new TimeSpan(endDateTick - today.Ticks).TotalSeconds >= 1));
            var chkCount = (apply.LimitCount > apply.AlreadyRegistered);
            bool result = (!chkDate || !chkCount) ? false : true;
            return result;
        }

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<TrainListDataModel> data)
        {
            var r = data.Where(s => s.ActivityName.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(DateTime beginDate, DateTime endDate, ref List<TrainListDataModel> data)
        {
            var r = data.Where(s => s.ActivityDateBegin >= beginDate && s.ActivityDateBegin <= endDate).ToList();
            data = r;
        }

        /// <summary>
        /// 列表排序功能
        /// </summary>
        /// <param name="sortCloumn"></param>
        /// <param name="data"></param>
        private void ListSort(string sortCloumn, ref List<TrainListDataModel> data)
        {
            switch (sortCloumn)
            {
                default:
                    data = data.OrderByDescending(o => o.Sort).ThenByDescending(o => o.ActivityDateBegin).ToList();
                    break;
            }
        }

        /// <summary>
        /// [前台] 列表分頁處理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public TrainListResultModel ListPagination(ref TrainListResultModel model, int page, int pageSize)
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

        public int SaveTrainApplyData(int trainID, TrainApplyDataModel model)
        {
            int identityID = 0;
            研討會報名主檔 applyMain;
            研討會報名對應檔 applyMap;
            List<研討會報名人員> Participants = new List<研討會報名人員>();

            if (!model.ID.HasValue)
            {
                applyMain = new 研討會報名主檔();
                Participants = new List<研討會報名人員>();
            }
            else
            {
                applyMain = this.DB.研討會報名主檔.Where(o => o.主索引 == model.ID).FirstOrDefault();
                Participants = this.DB.研討會報名人員.Where(o => o.對應報名檔主索引 == model.ID).ToList();
            }
            applyMain.公司傳真 = model.CompanyFax;
            applyMain.公司名稱 = model.CompanyName;
            applyMain.公司統編 = model.BusinessNo;
            applyMain.公司電話 = model.CompanyPhone;
            applyMain.同意條文 = model.Agree;
            applyMain.用戶編號 = model.UserNo;
            applyMain.統編抬頭 = model.BusinessNoTitle;
            applyMain.聯絡人Email = model.Email;
            applyMain.聯絡人姓名 = model.ContactPerson;
            applyMain.聯絡人手機 = model.MobilePhone;
            applyMain.聯絡人生日 = model.Birthday;

            if (!model.ID.HasValue)
            {
                this.DB.研討會報名主檔.Add(applyMain);
                this.DB.SaveChanges();
                applyMap = new 研討會報名對應檔()
                {
                    報名檔主索引 = applyMain.主索引,
                    對應研討會主索引 = trainID,
                    報名用戶編號 = UserFrontProvider.Instance.User.UserAccount
                };
                this.DB.研討會報名對應檔.Add(applyMap);
                this.DB.SaveChanges();
                identityID = applyMap.主索引;
                foreach (var pa in model.ParticipantsData)
                {
                    if (string.IsNullOrEmpty(pa.Name) && string.IsNullOrEmpty(pa.JobTitle))
                        continue;
                    研討會報名人員 temp = new 研討會報名人員()
                    {
                        參加人員姓名 = pa.Name,
                        參加人員職稱 = pa.JobTitle,
                        是否為聯絡人 = pa.IsContactPerson,
                        膳食種類 = pa.DietTypeValue,
                        對應報名檔主索引 = identityID,
                        報名狀態 = 1
                    };
                    Participants.Add(temp);
                }
                foreach (var p in Participants)
                    PublicMethodRepository.FilterXss(p);

                this.DB.研討會報名人員.AddRange(Participants);
                try
                {
                    this.DB.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                this.DB.Entry(applyMain).State = EntityState.Modified;
                this.DB.SaveChanges();
            }
            return identityID;
        }

        public int SaveTrainApplyDataFromUser(TrainApplyDataModel model)
        {
            #region 主檔更新

            int trainID = 0;
            研討會報名主檔 applyMain = this.DB.研討會報名主檔.Where(o => o.主索引 == model.ID).FirstOrDefault();
            applyMain.公司傳真 = model.CompanyFax;
            applyMain.公司名稱 = model.CompanyName;
            applyMain.公司統編 = model.BusinessNo;
            applyMain.公司電話 = model.CompanyPhone;
            applyMain.同意條文 = model.Agree;
            applyMain.統編抬頭 = model.BusinessNoTitle;
            applyMain.聯絡人姓名 = model.ContactPerson;
            applyMain.聯絡人手機 = model.MobilePhone;
            applyMain.更新時間 = DateTime.UtcNow.AddHours(8);
            PublicMethodRepository.FilterXss(applyMain);

            //applyMain.聯絡人Email = model.Email;
            //applyMain.聯絡人生日 = model.Birthday;
            //applyMain.用戶編號 = model.UserNo;
            this.DB.Entry(applyMain).State = EntityState.Modified;
            this.DB.SaveChanges();
            trainID = this.DB.研討會報名對應檔.Where(o => o.報名檔主索引 == model.ID).First().對應研討會主索引;

            #endregion 主檔更新

            List<int> participantsBeforeIDList = new List<int>();
            List<研討會報名人員> newPaData = new List<研討會報名人員>();

            #region 報名人員判斷

            foreach (var pa in model.ParticipantsData)
            {
                if (!string.IsNullOrEmpty(pa.Name) && !string.IsNullOrEmpty(pa.JobTitle))
                {
                    if (pa.BeforeID != null)
                        participantsBeforeIDList.Add((int)pa.BeforeID);
                    else
                    {
                        研討會報名人員 newPa = new 研討會報名人員()
                        {
                            參加人員姓名 = pa.Name,
                            參加人員職稱 = pa.JobTitle,
                            報名狀態 = 1,
                            對應報名檔主索引 = (int)model.ID,
                            膳食種類 = pa.DietTypeValue,
                            是否為聯絡人 = false
                        };
                        newPaData.Add(newPa);
                    }
                }
            }

            #endregion 報名人員判斷

            #region 若全部取消刪除所有人員

            if (participantsBeforeIDList.Count == 0 && newPaData.Count == 0)
            {
                this.DB.研討會報名人員.RemoveRange(this.DB.研討會報名人員.Where(o => o.對應報名檔主索引 == model.ID).ToList());
                this.DB.SaveChanges();
                return trainID;
            }

            #endregion 若全部取消刪除所有人員

            #region 修改原有人員料

            if (participantsBeforeIDList.Count > 0)
            {
                #region 避免後台移除判斷

                List<int> removeIndex = new List<int>();
                foreach (var id in participantsBeforeIDList)
                {
                    int index = participantsBeforeIDList.IndexOf(id);
                    var chk = this.DB.研討會報名人員.Where(o => o.主索引 == id).FirstOrDefault();
                    if (chk == null)
                        removeIndex.Add(index);
                }
                if (removeIndex.Count > 0)
                    participantsBeforeIDList = participantsBeforeIDList.Where(o => removeIndex.Contains(o)).ToList();

                #endregion 避免後台移除判斷

                this.DB.研討會報名人員.RemoveRange(this.DB.研討會報名人員.Where(o => !participantsBeforeIDList.Contains(o.主索引) && o.對應報名檔主索引 == model.ID).ToList());
                this.DB.SaveChanges();
            }

            var paList = this.DB.研討會報名人員.Where(o => o.對應報名檔主索引 == model.ID).ToList();

            foreach (var pa in participantsBeforeIDList)
            {
                var paModelData = model.ParticipantsData.Where(o => o.BeforeID == pa).First();
                var paData = this.DB.研討會報名人員.Where(o => o.主索引 == pa).First();
                paData.參加人員姓名 = paModelData.Name;
                paData.參加人員職稱 = paModelData.JobTitle;
                paData.膳食種類 = paModelData.DietTypeValue;
                paData.報名狀態 = paModelData.StatusCodeFromUser == "on" ? 0 : paData.報名狀態;
                PublicMethodRepository.FilterXss(pa);
                this.DB.Entry(paData).State = EntityState.Modified;
                this.DB.SaveChanges();
            }

            #endregion 修改原有人員料

            #region 加入新人員 新加入判斷目前報名人數 決定狀態 並啟動交易模式寫入

            if (newPaData.Count > 0)
            {
                using (var context = new DBEnergy())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var newApply in newPaData)
                            {
                                int remainingCount = GetTrainCurrentRemainingCount(context, trainID);
                                if (remainingCount > 0)
                                    newApply.報名狀態 = 1;
                                else
                                    newApply.報名狀態 = 2;
                                PublicMethodRepository.FilterXss(newApply);
                            }
                            context.研討會報名人員.AddRange(newPaData);
                            context.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            dbContextTransaction.Rollback();
                            throw new Exception("報名失敗，請洽管理員" + ex.Message);
                        }
                    }
                }
            }

            #endregion 加入新人員 新加入判斷目前報名人數 決定狀態 並啟動交易模式寫入

            return trainID;
        }

        /// <summary>
        /// 計算剩餘可報名人數
        /// </summary>
        /// <param name="trainID"></param>
        /// <returns></returns>
        private int GetTrainCurrentRemainingCount(DBEnergy dbContext, int trainID)
        {
            int openNumber = (int)dbContext.研討會明細檔.Where(o => o.對應研討會主索引 == trainID).First().活動人數上限;
            int current = this.DB.研討會報名主檔
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
                                               .Select(s => s.participateData.報名狀態 == 1).Count();

            int math = (openNumber - current);
            return math;
        }

        public TrainApplyDataModel GetTrainApplyByID(int trainID, int? applyID)
        {
            var apply = this.DB.研討會報名主檔
            .Join(
                 this.DB.研討會報名對應檔,
                main => main.主索引,
                map => map.報名檔主索引,
                (main, map) => new { Main = main, Map = map })
                .Where(s => s.Map.報名檔主索引 == applyID &&
                s.Map.報名用戶編號 == UserFrontProvider.Instance.User.UserAccount &&
                s.Map.對應研討會主索引 == trainID)
                .ToList()
                .Select(o => new TrainApplyDataModel()
                {
                    ID = o.Main.主索引,
                    Agree = o.Main.同意條文,
                    Birthday = o.Main.聯絡人生日,
                    BusinessNo = o.Main.公司統編,
                    BusinessNoTitle = o.Main.統編抬頭,
                    CompanyFax = o.Main.公司傳真,
                    CompanyName = o.Main.公司名稱,
                    CompanyPhone = o.Main.公司電話,
                    ContactPerson = o.Main.聯絡人姓名,
                    Email = o.Main.聯絡人Email,
                    MobilePhone = o.Main.聯絡人手機,
                    UserNo = o.Main.用戶編號,
                    ParticipantsData = GetTrainParticipants((int)applyID),
                    UpdateTime = o.Main.更新時間 == null ? DateTime.UtcNow.AddHours(8) : (DateTime)o.Main.更新時間
                })
                .FirstOrDefault();
            PublicMethodRepository.HtmlDecode(apply);

            return apply;
        }

        public List<Participants> GetTrainParticipants(int applyID)
        {
            List<Participants> result = new List<Participants>();
            var participants = this.DB.研討會報名人員.Where(o => o.對應報名檔主索引 == applyID).ToList();
            foreach (var pa in participants)
            {
                PublicMethodRepository.HtmlDecode(pa);
                result.Add(new Participants()
                {
                    ID = pa.主索引,
                    Name = pa.參加人員姓名,
                    JobTitle = pa.參加人員職稱,
                    IsContactPerson = pa.是否為聯絡人 ?? false,
                    DietTypeValue = pa.膳食種類,
                    StatusCode = pa.報名狀態 ?? 1
                });
            }
            return result;
        }

        public TrainContentModel GetTrainContentByID(int ID)
        {
            TrainContentModel result = new TrainContentModel();
            研討會主檔 data = DB.研討會主檔.Where(w => w.主索引 == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("無法取得研討會.");
            result.ActivityName = data.研討會名稱;

            研討會明細檔 details = DB.研討會明細檔.Where(o => o.對應研討會主索引 == ID).FirstOrDefault();
            if (details == null)
                throw new Exception("無法取得研討會.");
            result.ID = data.主索引;
            result.ActivityName = data.研討會名稱;
            result.Organizer = details.主辦單位;
            result.CoOrganiser = details.協辦單位;
            result.DeadlineBegin = details.報名期限_起;
            result.DeadlineEnd = details.報名期限_迄;
            result.EnrollmentRestrictions = (int)details.活動人數上限;
            result.SingleEnrollmentRestrictions = (int)details.用戶參加人數限制;
            result.ActivityContent = details.活動內容;
            result.ActivityLocation = details.活動地點;
            result.ActivityDate = string.Format("{0}~{1} {2}",
                details.活動日期起.Value.ToString("yyyy\\/MM\\/dd"),
                details.活動日期訖.Value.ToString("yyyy\\/MM\\/dd"),
                details.活動時間範圍);
            result.ChargesStatus = details.收費狀態 ?? false;
            //result.ActivityTimeRange = details.活動時間範圍;
            result.ContactPerson = details.聯絡人;
            result.ContactPhoneNumber = details.連絡電話;
            result.Remarks = details.備註;
            result.CreateDate = data.建立日期;
            result.IsStopRegistering = CheckRegistrationStatusByTrainID(ID);
            PublicMethodRepository.HtmlDecode(result);
            return result;
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