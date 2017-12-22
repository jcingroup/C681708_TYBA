using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.ExportExcelModels.TrainSignListModels;
using OutWeb.Models.Manage.ManageTrainApplyModels;
using OutWeb.Models.Manage.ManageTrainApplyModels.TrainApplyDetailsModels;
using OutWeb.Repositories;
using OutWeb.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 最新消息列表模組
    /// </summary>
    public class TrainApplyModule : ListModuleService
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public void SaveApplyParticipantsData(TrainApplyViewModel model)
        {
            using (var context = new DBEnergy())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        List<研討會報名人員> deleteList = new List<研討會報名人員>();
                        var applyMain = context.研討會報名對應檔
                        .Where(o => o.對應研討會主索引 == model.TrainID
                        && o.報名檔主索引 == model.ApplyID).FirstOrDefault();
                        if (applyMain == null)
                            throw new Exception("查無該報名檔");
                        if (model.Participants.Count > 0)
                        {
                            var applyParticipantsData = context.研討會報名人員.Where(o => o.對應報名檔主索引 == applyMain.報名檔主索引).ToList();
                            foreach (var pa in model.Participants)
                            {
                                int index = model.Participants.IndexOf(pa) + 1;
                                var paData = applyParticipantsData.Where(o => o.主索引 == pa.ID).FirstOrDefault();
                                if (paData != null)
                                {
                                    if (pa.Status == 0)
                                        deleteList.Add(paData);
                                    paData.報名狀態 = pa.Status;
                                    paData.繳費狀態 = pa.ChargesStatus == 1 ? true : false;
                                }
                                else
                                    throw new Exception("查無列表序號" + index + "報名人員，是否已被刪除?");
                                PublicMethodRepository.FilterXss(paData);
                                context.Entry(paData).State = EntityState.Modified;
                                context.SaveChanges();
                            }
                        }
                        if (deleteList.Count > 0)
                            context.研討會報名人員.RemoveRange(deleteList);
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

        public List<TrainApplyParticipants> GetTrainParticipants(int applyID)
        {
            List<TrainApplyParticipants> result = new List<TrainApplyParticipants>();
            var participants = this.DB.研討會報名人員.Where(o => o.對應報名檔主索引 == applyID).ToList();
            foreach (var pa in participants)
            {
                PublicMethodRepository.HtmlDecode(pa);
                result.Add(new TrainApplyParticipants()
                {
                    ID = pa.主索引,
                    Name = pa.參加人員姓名,
                    JobTitle = pa.參加人員職稱,
                    IsContactPerson = pa.是否為聯絡人 ?? false,
                    DietTypeValue = pa.膳食種類,
                    StatusCode = pa.報名狀態 ?? 1,
                    IsCharges = pa.繳費狀態 ?? false
                });
            }
            return result;
        }

        public override object DoGetDetailsByID(int trainID)
        {
            TrainApplyDetailsModel result = new TrainApplyDetailsModel();

            #region 上方明細

            研討會主檔 mainData = DB.研討會主檔.Where(w => w.主索引 == trainID).FirstOrDefault();
            if (mainData == null)
                throw new Exception("無法取得研討會.");
            result.Data.ActivityName = mainData.研討會名稱;

            研討會明細檔 details = DB.研討會明細檔.Where(o => o.對應研討會主索引 == trainID).FirstOrDefault();
            if (details == null)
                throw new Exception("無法取得研討會.");
            result.Data.ID = mainData.主索引;
            result.Data.ActivityName = mainData.研討會名稱;
            result.Data.Organizer = details.主辦單位;
            result.Data.CoOrganiser = details.協辦單位;
            result.Data.DeadlineBegin = details.報名期限_起;
            result.Data.DeadlineEnd = details.報名期限_迄;
            result.Data.EnrollmentRestrictions = (int)details.活動人數上限;
            result.Data.SingleEnrollmentRestrictions = (int)details.用戶參加人數限制;
            result.Data.Sort = (int)details.排序;
            result.Data.ActivityContent = details.活動內容;
            result.Data.ActivityLocation = details.活動地點;
            result.Data.ActivityDateBegin = (DateTime)details.活動日期起;
            result.Data.ActivityDateBegin = (DateTime)details.活動日期訖;
            result.Data.ActivityTimeRange = details.活動時間範圍;
            result.Data.ActivityStatus = (bool)details.顯示狀態;
            //result.Data.SignUpStatus = (bool)details.報名狀態;
            result.Data.ContactPerson = details.聯絡人;
            result.Data.ContactPhoneNumber = details.連絡電話;
            result.Data.Remarks = details.備註;
            result.Data.AlreadyRegisteredCount = MathAlreadyRegisteredCount(trainID);
            result.Data.MeatCount = StatisticsFoodType(trainID, DietCategory.Meat);
            result.Data.VegetarianCount = StatisticsFoodType(trainID, DietCategory.Vegetarian);
            result.Data.DisplayHome = (bool)details.首頁顯示;
            result.Data.ChargesSatus = (bool)details.收費狀態;
            #endregion 上方明細

            #region 下方列表

            result.List = this.DB.研討會報名主檔
                                    .Join(
                                    this.DB.研討會報名對應檔,
                                    main => main.主索引,
                                    map => map.報名檔主索引,
                                    (main, map) => new { Main = main, Map = map })
                                    .Where(w => w.Map.對應研討會主索引 == trainID)
                                    .ToList()
                                    .Select(s => new TrainApplyDataListModel()
                                    {
                                        CompanyName = s.Main.公司名稱,
                                        CompanyPhone = s.Main.公司電話,
                                        ContactPerson = s.Main.聯絡人姓名,
                                        ID = s.Main.主索引,
                                        MapTrainID = s.Map.對應研討會主索引,
                                        PaymentStatus = "免費",
                                        RegistrationCount = MathAlreadyRegisteredCount(trainID, s.Map.報名檔主索引),
                                        RegistrationSuccessCount = EnrollmentStatusStatistics(trainID, s.Map.報名檔主索引, 1),
                                        RegistrationAlternateCount = EnrollmentStatusStatistics(trainID, s.Map.報名檔主索引, 2),
                                    })
                                    .ToList();

            #endregion 下方列表
            PublicMethodRepository.HtmlDecode(result);
            foreach (var item in result.List)
                PublicMethodRepository.HtmlDecode(item);
            return result;
        }

        /// <summary>
        /// 取得該研討會所完成的繳費人數
        /// </summary>
        /// <param name="applyID"></param>
        /// <returns></returns>
        private int GetPayOffCountOfTrainByTrainID(int trainID)
        {
            int payOffCount = 0;
            List<int> applyList = this.DB.研討會報名對應檔.Where(o => o.對應研討會主索引 == trainID)
                .Select(o => o.報名檔主索引)
                .ToList();
            if (applyList.Count > 0)
            {
                payOffCount = this.DB.研討會報名人員.Where(o => applyList.Contains(o.對應報名檔主索引) && o.繳費狀態 == true).Count();
            }
            return payOffCount;
        }

        /// <summary>
        /// 刪除報名檔
        /// </summary>
        /// <param name="trainID"></param>
        /// <param name="applyID"></param>
        public bool DeleteApply(int trainID, int applyID)
        {
            bool delIsSuccess = true;
            using (var context = new DBEnergy())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.研討會報名人員.RemoveRange(context.研討會報名人員.Where(o => o.對應報名檔主索引 == applyID));
                        context.研討會報名對應檔.RemoveRange(context.研討會報名對應檔.Where(o => o.報名檔主索引 == applyID && o.對應研討會主索引 == trainID));
                        context.研討會報名主檔.RemoveRange(context.研討會報名主檔.Where(o => o.主索引 == applyID));
                        context.SaveChanges();
                        dbContextTransaction.Commit();
                    }
                    catch
                    {
                        delIsSuccess = false;
                        dbContextTransaction.Rollback();
                    }
                    return delIsSuccess;
                }
            }
        }

        /// <summary>
        /// 取得每個研討的報名人員列表(全部)
        /// </summary>
        /// <param name="trainID"></param>
        /// <param name="applyID"></param>
        /// <returns></returns>
        public ReplyDataModel GetTrainApplyParticipantsDataByTrainID(int trainID)
        {
            ReplyDataModel model = new ReplyDataModel();

            var applyData = this.DB.研討會報名主檔
                .Join(
                     this.DB.研討會報名對應檔,
                    main => main.主索引,
                    map => map.報名檔主索引,
                    (main, map) => new { Main = main, Map = map })
                    .Where(o => o.Map.對應研討會主索引 == trainID)
                     .Select(o => new Data()
                     {
                         TrainID = trainID,
                         ID = o.Main.主索引,
                         CompanyName = o.Main.公司名稱,
                         CompanyPhone = o.Main.公司電話,
                         Email = o.Main.聯絡人Email,
                         UserNo = o.Main.用戶編號,
                     })
                    .ToList();

            foreach (var apply in applyData)
            {
                var pa = GetTrainParticipants((int)apply.ID);
                apply.ParticipantsData = pa;
            }
            model.Data = applyData;
            PublicMethodRepository.HtmlDecode(model);
            foreach (var d in model.Data)
            {
                PublicMethodRepository.HtmlDecode(d);
                foreach (var p in d.ParticipantsData)
                    PublicMethodRepository.HtmlDecode(p);
            }

            return model;
        }

        /// <summary>
        /// 取得每個報名檔的報名人員列表
        /// </summary>
        /// <param name="trainID"></param>
        /// <param name="applyID"></param>
        /// <returns></returns>
        public TrainApplyDataModel GetTrainApplyParticipantsDataByApplyID(int trainID, int? applyID)
        {
            TrainApplyDataModel model = new TrainApplyDataModel();
            List<TrainApplyParticipants> participants = new List<TrainApplyParticipants>();
            var apply = this.DB.研討會報名主檔
                .Join(
                     this.DB.研討會報名對應檔,
                    main => main.主索引,
                    map => map.報名檔主索引,
                    (main, map) => new { Main = main, Map = map })
                     .Where(s => s.Map.報名檔主索引 == applyID &&
                    s.Map.對應研討會主索引 == trainID);

            model = apply
                    .Select(o => new TrainApplyDataModel()
                    {
                        TrainID = trainID,
                        ID = o.Main.主索引,
                        CompanyName = o.Main.公司名稱,
                        CompanyPhone = o.Main.公司電話,
                        Email = o.Main.聯絡人Email,
                        UserNo = o.Main.用戶編號,
                        ContactName = o.Main.聯絡人姓名,
                        ContactPhone = o.Main.聯絡人手機,
                    }).First();

            var paData =
                apply.Join(
                    this.DB.研討會報名人員,
                    mainData => mainData.Main.主索引,
                    participate => participate.對應報名檔主索引,
                    (data, participateData) => new { data, participateData })
                    .Where(s => s.data.Map.報名檔主索引 == applyID &&
                    s.data.Map.對應研討會主索引 == trainID)
                    .ToList()
                    .Select(o => new TrainApplyDataModel()
                    {
                        ParticipantsData = GetTrainParticipants((int)applyID)
                    })
                    .FirstOrDefault();
            paData = paData ?? new TrainApplyDataModel();
            model.ParticipantsData = paData.ParticipantsData;
            PublicMethodRepository.HtmlDecode(model);
            foreach (var p in model.ParticipantsData)
                PublicMethodRepository.HtmlDecode(p);
            model.ChargesStatus = GetChargesStatusByTrainID(trainID);
            return model ?? new TrainApplyDataModel();
        }

        /// <summary>
        /// 取得研討會收費狀況
        /// </summary>
        /// <param name="trainID"></param>
        /// <returns></returns>
        public bool GetChargesStatusByTrainID(int trainID)
        {
            bool isCharges = this.DB.研討會明細檔.Where(o => o.對應研討會主索引 == trainID).First().收費狀態 ?? false;
            return isCharges;
        }

        /// <summary>
        /// 報名人數狀態統計
        /// </summary>
        /// <param name="trainID">研討會主索引</param>
        /// <param name="applyID">報名檔主索引</param>
        /// <param name="statusCode">0=>取消報名 1=>完成報名 2=>額滿候補</param>
        /// <returns></returns>

        private int EnrollmentStatusStatistics(int trainID, int applyID, int statusCode)
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
                                    && w.participateData.報名狀態 == statusCode).Count();
            return count;
        }

        /// <summary>
        /// 報名人數統計
        /// </summary>
        /// <param name="trainID"></param>
        /// <param name="applyID"></param>
        /// <returns></returns>
        private int MathAlreadyRegisteredCount(int trainID, int? applyID = null)
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
                                    && applyID == null ? true : w.data.Map.報名檔主索引 == applyID)
                                    .Select(s => s.participateData).Count();
            return count;
        }

        /// <summary>
        /// 膳食統計
        /// </summary>
        /// <param name="trainID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private int StatisticsFoodType(int trainID, DietCategory type)
        {
            var participateList = this.DB.研討會報名主檔
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
                        .Select(s => s.participateData).ToList();

            int count = participateList.Where(o => o.膳食種類 == type.ToString()).Count();
            return count;
        }

        public override object DoGetList<TFilter>(TFilter model, Language language)
        {
            TrainApplyListFilterModel filterModel = (model as TrainApplyListFilterModel);
            PublicMethodRepository.FilterXss(filterModel);
            TrainApplyListResultModel result = new TrainApplyListResultModel();
            List<TrainApplyStandardDataModel> data = new List<TrainApplyStandardDataModel>();
            try
            {
                data =
                     this.DB.研討會主檔.Join(this.DB.研討會明細檔,
                       t1 => t1.主索引,
                       t2 => t2.對應研討會主索引,
                       (main, details) => new { Main = main, Details = details })
                       .ToList()
                       .Select(s => new TrainApplyStandardDataModel()
                       {
                           ID = s.Main.主索引,
                           Organizer = s.Details.主辦單位,
                           CoOrganiser = s.Details.協辦單位,
                           DeadlineBegin = s.Details.報名期限_起,
                           DeadlineEnd = s.Details.報名期限_迄,
                           EnrollmentRestrictions = (int)s.Details.活動人數上限,
                           SingleEnrollmentRestrictions = (int)s.Details.用戶參加人數限制,
                           AlreadyRegisteredCount = MathAlreadyRegisteredCount(s.Main.主索引),
                           ActivityName = s.Main.研討會名稱,
                           CreateDate = s.Main.建立日期,
                           Sort = (int)s.Details.排序,
                           ActivityContent = s.Details.活動內容,
                           ActivityLocation = s.Details.活動地點,
                           ActivityDateBegin = (DateTime)s.Details.活動日期起,
                           ActivityDateEnd = (DateTime)s.Details.活動日期訖,
                           ActivityTimeRange = s.Details.活動時間範圍,
                           ActivityStatus = (bool)s.Details.顯示狀態,
                           ChargesSatus = (bool)s.Details.收費狀態,
                           //SignUpStatus = (bool)s.Details.報名狀態,
                           ContactPerson = s.Details.聯絡人,
                           ContactPhoneNumber = s.Details.連絡電話,
                           Remarks = s.Details.備註,
                           MeatCount = StatisticsFoodType(s.Main.主索引, DietCategory.Meat),
                           VegetarianCount = StatisticsFoodType(s.Main.主索引, DietCategory.Vegetarian),
                           DisplayHome = (bool)s.Details.首頁顯示,
                           PayOffCount = GetPayOffCountOfTrainByTrainID(s.Main.主索引)

                       })
                       .ToList();
                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }
                //發佈日期搜尋
                if (!string.IsNullOrEmpty(filterModel.ActivityBeginDate) && !string.IsNullOrEmpty(filterModel.ActivityEndDate))
                {
                    this.ListDateFilter(Convert.ToDateTime(filterModel.ActivityBeginDate), Convert.ToDateTime(filterModel.ActivityEndDate), ref data);
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

        //private void NewsListFilterLanguage(Language language, ref List<NewsListDataModel> data)
        //{
        //    var r = data.Where(s => s.Language == language.GetCode()).ToList();
        //    data = r;
        //}

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
        private void ListFilter(string filterStr, ref List<TrainApplyStandardDataModel> data)
        {
            var r = data.Where(s => s.ActivityName.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(DateTime beginDate, DateTime endDate, ref List<TrainApplyStandardDataModel> data)
        {
            var r = data.Where(s => s.ActivityDateBegin >= beginDate && s.ActivityDateBegin <= endDate).ToList();
            data = r;
        }

        /// <summary>
        /// 前台顯示搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListStatusFilter(string status, ref List<TrainApplyStandardDataModel> data)
        {
            if (status == "Y")
                data = data.Where(s => s.ActivityStatus == true).ToList();
            else
                data = data.Where(s => s.ActivityStatus == false).ToList();
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<TrainApplyStandardDataModel> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, ref List<TrainApplyStandardDataModel> data)
        {
            switch (sortCloumn)
            {
                case "sortDate/asc":
                    data = data.OrderBy(o => o.ActivityDateBegin).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortDate/desc":
                    data = data.OrderByDescending(o => o.ActivityDateBegin).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortStatus/asc":
                    data = data.OrderBy(o => o.ActivityStatus).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortStatus/desc":
                    data = data.OrderByDescending(o => o.ActivityStatus).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortIndex/asc":
                    data = data.OrderBy(o => o.Sort).ThenByDescending(g => g.ActivityDateBegin).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.Sort).ThenByDescending(g => g.ActivityDateBegin).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.Sort).ToList();
                    break;
            }
        }

        public override int DoSaveData(FormCollection form, Language language, int? ID = default(int?), List<HttpPostedFileBase> image = null, List<HttpPostedFileBase> images = null)
        {
            throw new NotImplementedException();
        }

        public override void DoDeleteByID(int ID)
        {
            throw new NotImplementedException();
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