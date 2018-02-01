using Newtonsoft.Json;
using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.ActivityModels;
using OutWeb.Models.Manage.ApplyMaintainModels;
using OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels;
using OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyDetailsListModels;
using OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyModalModels;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 最新消息列表模組
    /// </summary>
    public class ApplyMaintainModule : IDisposable
    {
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
        { get { return this.m_DB; } set { this.m_DB = value; } }



        private List<MemberInfo> GetMemberInfoByApplyID(int actID, int applyID)
        {
            List<MemberInfo> memberList = new List<MemberInfo>();
            var mamber = DB.APPLY_MEMBER.Where(o => o.REF_ACT_ID == actID && o.MAP_APPLY_ID == applyID).ToList();
            foreach (var mb in mamber)
            {
                memberList.Add(new MemberInfo()
                {
                    ID = mb.ID,
                    MemberName = mb.MERBER_NM,
                    MemberBirthday = mb.MEMBER_BIRDT,
                    MemberIdentityID = mb.MEMBER_IDEN_ID,
                    MemberType = mb.MERBER_TP
                });
            }
            return memberList;
        }

        public ApplyModalDataModel GetApplyData(int actID, int applyID)
        {
            ApplyModalDataModel model =
                        DB.OLACT
                       .Join(DB.APPLY,
                           act => act.ID,
                           apply => apply.MAP_ACT_ID,
                             (act, apply) => new { act, apply })
                            .Where(o => o.act.ID == actID && o.apply.ID == applyID)
                             .AsEnumerable()
                             .Select(s => new ApplyModalDataModel()
                             {
                                 ActivityID = s.act.ID,
                                 ApplyID = s.apply.ID,
                                 ActivityTitle = s.act.ACTITLE,
                                 ApplyGroupID = s.apply.MAP_ACT_GUP_ID,
                                 ApplyNumber = s.apply.APPLY_IDEN_NUM,
                                 ApplyStatus = s.apply.APPLY_SUCCESS,
                                 Coach = s.apply.TEAM_COACH,
                                 Contact = s.apply.CONTACT,
                                 ContactPhone = s.apply.CONTACT_PHONE,
                                 Email = s.apply.EMAIL,
                                 Remark = s.apply.REMRK.ReplaceEmpty(),
                                 TeamName = s.apply.TEAM_NM,
                                 Member = GetMemberInfoByApplyID(s.act.ID, s.apply.ID)
                             })
                             .FirstOrDefault();

            if (model != null)
            {
                var group = DB.OLACTGROUP
                                .Where(o => o.MAP_ACT_ID == model.ActivityID)
                                .Select(s => new OutWeb.Models.FrontEnd.ApplyModels.ApplyViewGroup()
                                {
                                    GroupID = s.ID,
                                    GroupName = s.GROUP_NAME,
                                    GroupApplyLimit = s.TEAM_APPLY_LIMIT,
                                    CountApplyLimit = s.COUNT_APPLY_LIMIT
                                })
                                .ToList();

                foreach (var g in group)
                {
                    var applyGorupLimitCount = DB.APPLY.Where(o => o.MAP_ACT_ID == model.ActivityID && o.MAP_ACT_GUP_ID == g.GroupID).Count();
                    g.CountApplyLastLimit = (g.GroupApplyLimit - applyGorupLimitCount) <= 0 ? 0 : (g.GroupApplyLimit - applyGorupLimitCount);
                    model.ActivityGroup.Add(g);
                }
                model.ApplyGroupJsonString = JsonConvert.SerializeObject(model.ActivityGroup);
            }

            return model;
        }

        public void DoDeleteByID(int actID, int applyID)
        {
            using (var transction = DB.Database.CurrentTransaction ?? DB.Database.BeginTransaction())
            {
                var member = DB.APPLY_MEMBER.Where(o => o.REF_ACT_ID == actID && o.MAP_APPLY_ID == applyID).ToList();
                var apply = DB.APPLY.Where(o => o.MAP_ACT_ID == actID && o.ID == applyID).FirstOrDefault();
                if (apply == null)
                    throw new Exception("無法刪除該報名,是否已被刪除？");
                DB.APPLY_MEMBER.RemoveRange(member);
                DB.APPLY.Remove(apply);
                try
                {
                    DB.SaveChanges();
                    transction.Commit();
                }
                catch (Exception ex)
                {
                    transction.Rollback();

                    throw ex;
                }
            }
        }

        private Dictionary<int, string> GetActivityGroupListByID(int actID)
        {
            var data = DB.OLACTGROUP.Where(o => o.MAP_ACT_ID == actID)
                .OrderBy(o => o.ID)
                .ToDictionary(d => d.ID, d => d.GROUP_NAME);
            return data;
        }

        private GroupModel GetGroupInfoByID(int actID, int groupID)
        {
            var data = DB.OLACTGROUP
                .Where(o => o.MAP_ACT_ID == actID && o.ID == groupID)
                .AsEnumerable()
                .Select(s => new GroupModel()
                {
                    GroupID = s.ID,
                    GroupName = s.GROUP_NAME,
                    GroupApplyLimit = s.TEAM_APPLY_LIMIT,
                    MemberApplyLimit = s.COUNT_APPLY_LIMIT,
                    RegisteredSuccessCount = DB.APPLY.Where(o => o.MAP_ACT_ID == actID && o.MAP_ACT_GUP_ID == groupID && o.APPLY_SUCCESS == true).Count(),
                    RegisteredCount = DB.APPLY.Where(g => g.MAP_ACT_ID == actID && g.MAP_ACT_GUP_ID == groupID).Count()
                })
                .First();
            return data;
        }

        public ApplyDetailsDataModel GetApplyDetails(ApplyDetailsDataModel details)
        {
            details.GroupList = GetActivityGroupListByID(details.ActivityID);
            var activityData = DB.OLACT.Where(o => o.ID == details.ActivityID).FirstOrDefault();
            if (activityData == null)
                throw new Exception("無喇取得該活動賽事,是否已被移除？");

            details.ActivityID = activityData.ID;
            details.ActivityDateRange = activityData.ACT_DATE_DESC;
            details.ActivityName = activityData.ACTITLE;

            var applyLstData =
                DB.APPLY
                .Where(w => w.MAP_ACT_ID == details.ActivityID)
                .AsEnumerable()
                .Select(s => new ApplyDetailsListDataModel()
                {
                    ID = s.ID,
                    ApplyDate = s.BUD_DT.ConvertDateTimeTo10CodeString(),
                    ApplyNumber = s.APPLY_IDEN_NUM,
                    ApplySuccessStatus = s.APPLY_SUCCESS,
                    ApplyTeamName = s.TEAM_NM,
                    ApplyTeamMemberCount = DB.APPLY_MEMBER.Where(o => o.MAP_APPLY_ID == s.ID && o.REF_ACT_ID == details.ActivityID).Count(),
                    GroupID = s.MAP_ACT_GUP_ID,
                    ContactPhone = s.CONTACT_PHONE
                })
                .ToList();

            details.ListData.Result.Data = applyLstData;
            details.GroupInfo.RegisteredSuccessCount = DB.APPLY.Where(o => o.MAP_ACT_ID == details.ActivityID && o.APPLY_SUCCESS == true).Count();
            details.GroupInfo.RegisteredCount = DB.APPLY.Where(g => g.MAP_ACT_ID == details.ActivityID).Count();
            details.GroupInfo.GroupApplyLimit = DB.OLACTGROUP.Where(o => o.MAP_ACT_ID == details.ActivityID).Sum(m => m.TEAM_APPLY_LIMIT);

            if (details.ListData.Filter.GroupID.HasValue)
            {
                details.ListData.Result.Data = applyLstData.Where(o => o.GroupID == details.ListData.Filter.GroupID).ToList();
                //若要在上方訊息區塊秀出對應的組別資料 移除下面註解
                //details.GroupInfo = GetGroupInfoByID(details.ActivityID, (int)details.ListData.Filter.GroupID);
            }

            if (!string.IsNullOrEmpty(details.ListData.Filter.QueryString))
            {
                details.ListData.Result.Data = applyLstData.Where(o => o.ApplyTeamName.Contains(details.ListData.Filter.QueryString)).ToList();
            }

            //排序
            switch (details.ListData.Filter.SortColumn)
            {
                case "sortApplyStatus/asc":
                    details.ListData.Result.Data = details.ListData.Result.Data.OrderBy(o => o.ApplySuccessStatus).ThenByDescending(o => o.ApplyDate).ToList();
                    break;

                case "sortApplyStatus/desc":
                    details.ListData.Result.Data = details.ListData.Result.Data.OrderByDescending(o => o.ApplySuccessStatus).ThenByDescending(o => o.ApplyDate).ToList();
                    break;

                default:
                    details.ListData.Result.Data = details.ListData.Result.Data.OrderByDescending(o => o.ApplyDate).ThenByDescending(g => g.ApplySuccessStatus).ToList();
                    break;
            }
            PaginationResult pagination;
            var tempData = details.ListData.Result.Data;
            //分頁
            this.ListPageList(details.ListData.Filter.CurrentPage, ref tempData, out pagination);
            details.ListData.Result.Data = tempData;
            details.ListData.Result.Pagination = pagination;
            foreach (var d in details.ListData.Result.Data)
                PublicMethodRepository.HtmlDecode(d);
            return details;
        }

        public ApplyExcelReplyDataModel GetApplyDetailsForExcel(ApplyExcelReplyDataModel details)
        {
            details.GroupList = GetActivityGroupListByID(details.ActivityID);
            var activityData = DB.OLACT.Where(o => o.ID == details.ActivityID).FirstOrDefault();
            if (activityData == null)
                throw new Exception("無法取得該活動賽事,是否已被移除？");

            details.ActivityID = activityData.ID;
            details.ActivityName = activityData.ACTITLE;

            var applyLstData =
                DB.APPLY
                .Where(w => w.MAP_ACT_ID == details.ActivityID)
                .AsEnumerable()
                .Select(s => new ApplyListDetailsData()
                {
                    ID = s.ID,
                    ApplyDate = s.BUD_DT.ConvertDateTimeTo10CodeString(),
                    ApplyNumber = s.APPLY_IDEN_NUM,
                    ApplySuccessStatus = s.APPLY_SUCCESS,
                    ApplyTeamName = s.TEAM_NM,
                    ApplyTeamMemberCount = DB.APPLY_MEMBER.Where(o => o.MAP_APPLY_ID == s.ID && o.REF_ACT_ID == details.ActivityID).Count(),
                    GroupID = s.MAP_ACT_GUP_ID,
                    ContactPhone = s.CONTACT_PHONE,
                    ContactEmail = s.EMAIL,
                    Remark = s.REMRK.ReplaceEmpty(),
                    Coach = s.TEAM_COACH,
                    Contact = s.CONTACT,
                    MemberInfo = GetMemberInfoByApplyID(details.ActivityID, s.ID)
                })
                .ToList();

            details.ApplyListData = applyLstData;


            foreach (var d in details.ApplyListData)
                PublicMethodRepository.HtmlDecode(d);
            return details;
        }

        public ApplyModalDataModel SaveApply(ApplyModalDataModel model)
        {
            ApplyModalDataModel result = new ApplyModalDataModel();

            using (var transction = DB.Database.CurrentTransaction ?? DB.Database.BeginTransaction())
            {
                var apply = DB.APPLY.Where(o => o.MAP_ACT_ID == model.ActivityID && o.ID == model.ApplyID).FirstOrDefault();
                if (apply == null)
                    throw new Exception("無法取得該報名檔,是否已刪除？");

                apply.MAP_ACT_GUP_ID = model.ApplyGroupID;
                apply.TEAM_COACH = model.Coach;
                apply.CONTACT = model.Contact;
                apply.CONTACT_PHONE = model.ContactPhone;
                apply.EMAIL = model.Email;
                apply.REMRK = model.Remark.ReplaceEmpty();
                apply.TEAM_NM = model.TeamName;
                apply.MAP_ACT_ID = model.ActivityID;
                apply.APPLY_SUCCESS = model.ApplyStatus;
                apply.UPD_DT = DateTime.UtcNow.AddHours(8);

                try
                {
                    this.DB.Entry(apply).State = EntityState.Modified;
                    DB.SaveChanges();
                    transction.Commit();
                }
                catch (Exception ex)
                {
                    transction.Rollback();
                    throw ex;
                }
            }
            IQueryable<APPLY_MEMBER> deleteMember;
            if (model.Member.Count > 0)
            {
                List<int> memberIds = model.Member.Where(o => o.ID.HasValue || o.ID > 0).Select(s => (int)s.ID).ToList();

                using (var transaction = DB.Database.CurrentTransaction ?? DB.Database.BeginTransaction())
                {
                    if (memberIds.Count > 0)
                    {
                        var members = DB.APPLY_MEMBER.Where(o => memberIds.Contains(o.ID)).ToList();
                        foreach (var baseMb in members)
                        {
                            var updateMb = model.Member.Where(o => o.ID == baseMb.ID).First();
                            baseMb.MEMBER_IDEN_ID = updateMb.MemberIdentityID;
                            baseMb.MEMBER_BIRDT = updateMb.MemberBirthday;
                            baseMb.MERBER_NM = updateMb.MemberName;
                            baseMb.MERBER_TP = updateMb.MemberType;
                            baseMb.MAP_APPLY_ID = model.ApplyID;
                            baseMb.REF_ACT_ID = model.ActivityID;
                            baseMb.UPD_DT = DateTime.UtcNow.AddHours(8);
                            this.DB.Entry(baseMb).State = EntityState.Modified;
                            try
                            {
                                DB.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                throw;
                            }
                        }
                    }
                    else
                    {
                        deleteMember = DB.APPLY_MEMBER.Where(o => o.REF_ACT_ID == model.ActivityID && o.MAP_APPLY_ID == model.ApplyID);
                        DB.APPLY_MEMBER.RemoveRange(deleteMember);
                    }
                    deleteMember = DB.APPLY_MEMBER.Where(o => o.REF_ACT_ID == model.ActivityID && o.MAP_APPLY_ID == model.ApplyID && !memberIds.Contains(o.ID));
                    DB.APPLY_MEMBER.RemoveRange(deleteMember);

                    List<APPLY_MEMBER> memberList = new List<APPLY_MEMBER>();

                    foreach (var member in model.Member)
                    {
                        if (!member.ID.HasValue || member.ID == 0)
                        {
                            APPLY_MEMBER temp = new APPLY_MEMBER()
                            {
                                MEMBER_IDEN_ID = member.MemberIdentityID,
                                MEMBER_BIRDT = member.MemberBirthday,
                                MERBER_NM = member.MemberName,
                                MERBER_TP = member.MemberType,
                                BUD_DT = DateTime.UtcNow.AddHours(8),
                                UPD_DT = DateTime.UtcNow.AddHours(8),
                                MAP_APPLY_ID = model.ApplyID,
                                REF_ACT_ID = model.ActivityID
                            };
                            memberList.Add(temp);
                        }
                    };
                    DB.APPLY_MEMBER.AddRange(memberList);
                    DB.SaveChanges();

                    try
                    {
                        DB.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
            result = GetApplyData(model.ApplyID, model.ActivityID);
            return result;
        }

        public ApplyMaintainListResultModel DoGetList(ApplyMaintainListFilterModel filterModel)
        {
            ApplyMaintainListResultModel result = new ApplyMaintainListResultModel();
            List<ApplyMaintainListDataModel> data = new List<ApplyMaintainListDataModel>();
            try
            {
                data = DB.OLACT
                    .AsEnumerable()
                    .Select(s => new ApplyMaintainListDataModel()
                    {
                        ID = s.ID,
                        ActivityName = s.ACTITLE,
                        PublishDateString = s.PUB_DT_STR,
                        Sort = s.SQ,
                        ApplyStatus = ActivityStatusCheckByID(s.ID),
                        RegisteredCount = DB.APPLY.Where(g => g.MAP_ACT_ID == s.ID).Count(),
                        LimitCount = DB.OLACTGROUP.Where(o => o.MAP_ACT_ID == s.ID).Sum(m => m.TEAM_APPLY_LIMIT),
                        ActivityDateRange = s.ACT_DATE_DESC
                    })
                    .ToList();

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

        //public int DoSaveData(ActivityDataModel model)
        //{
        //    OLACT saveModel;
        //    FileRepository fileRepository = new FileRepository();
        //    if (!model.ID.HasValue)
        //    {
        //        saveModel = new OLACT();
        //        saveModel.BUD_ID = UserProvider.Instance.User.ID;
        //        saveModel.BUD_DT = DateTime.UtcNow.AddHours(8);
        //    }
        //    else
        //    {
        //        saveModel = this.DB.OLACT.Where(s => s.ID == model.ID).FirstOrDefault();
        //    }
        //    saveModel.ACTITLE = model.Title;
        //    saveModel.SQ = model.Sort;
        //    saveModel.PUB_DT_STR = model.PublishDateStr;
        //    saveModel.APPLY_DATE_BEGIN = model.ApplyDateTimeBegin;
        //    saveModel.APPLY_DATE_END = model.ApplyDateTimeEnd;
        //    saveModel.ACT_CONTENT = model.contenttext;
        //    saveModel.ACT_NUM = model.ActivityNumber;
        //    saveModel.ACT_DATE_DESC = model.ActivityDateTimeDescription;
        //    saveModel.DISABLE = model.Disable;
        //    saveModel.UPD_ID = UserProvider.Instance.User.ID;
        //    saveModel.UPD_DT = DateTime.UtcNow.AddHours(8);
        //    PublicMethodRepository.FilterXss(saveModel);

        //    if (!model.ID.HasValue)
        //    {
        //        this.DB.OLACT.Add(saveModel);
        //    }
        //    else
        //    {
        //        this.DB.Entry(saveModel).State = EntityState.Modified;
        //    }

        //    try
        //    {
        //        this.DB.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    int identityId = (int)saveModel.ID;

        //    #region 組別

        //    List<OLACTGROUP> groupUpdateEntity = new List<OLACTGROUP>();
        //    List<OLACTGROUP> groupAddEntity = new List<OLACTGROUP>();
        //    List<int> oldIds = model.ActivityGroup.Where(o => o.GroupID > 0).Select(s => s.GroupID).ToList();
        //    using (var transation = DB.Database.CurrentTransaction ?? DB.Database.BeginTransaction())
        //    {
        //        var removeNotInEntities = DB.OLACTGROUP.Where(o => o.MAP_ACT_ID == identityId && !oldIds.Contains(o.ID));
        //        if (removeNotInEntities.Count() > 0)
        //            DB.OLACTGROUP.RemoveRange(removeNotInEntities);

        //        foreach (var group in model.ActivityGroup)
        //        {
        //            OLACTGROUP temp = null;
        //            if (group.GroupID == 0)
        //            {
        //                temp = new OLACTGROUP();

        //                temp.BUD_DT = DateTime.UtcNow.AddHours(8);
        //                temp.BUD_ID = UserProvider.Instance.User.ID;
        //            }
        //            else
        //            {
        //                temp = DB.OLACTGROUP.Where(o => o.ID == group.GroupID).First();
        //            }
        //            temp.MAP_ACT_ID = identityId;
        //            temp.GROUP_NAME = group.GroupName;
        //            temp.TEAM_APPLY_LIMIT = group.GroupApplyLimit;
        //            temp.COUNT_APPLY_LIMIT = group.CountApplyLimit;
        //            temp.UPD_DT = DateTime.UtcNow.AddHours(8);
        //            temp.UPD_ID = UserProvider.Instance.User.ID;

        //            if (group.GroupID == 0)
        //            {
        //                groupAddEntity.Add(temp);
        //            }
        //            else
        //            {
        //                groupUpdateEntity.Add(temp);
        //            }
        //        }

        //        foreach (var item in groupUpdateEntity)
        //        {
        //            this.DB.Entry(item).State = EntityState.Modified;
        //        }

        //        if (groupAddEntity.Count > 0)
        //            this.DB.OLACTGROUP.AddRange(groupAddEntity);
        //        try
        //        {
        //            DB.SaveChanges();
        //            transation.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            transation.Rollback();
        //            throw ex;
        //        }
        //    }

        //    #endregion 組別

        //    #region 檔案處理

        //    FilesModel fileModel = new FilesModel()
        //    {
        //        ActionName = "Activity",
        //        ID = identityId,
        //        OldFileIds = model.OldFilesId
        //    };

        //    fileRepository.UploadFile("Post", fileModel, model.Files, "M");
        //    fileRepository.SaveFileToDB(fileModel);

        //    #endregion 檔案處理

        //    return identityId;
        //}

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList<T>(int currentPage, ref List<T> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, ref List<ApplyMaintainListDataModel> data)
        {
            switch (sortCloumn)
            {
                case "sortApplyStatus/asc":
                    data = data.OrderBy(o => o.ApplyStatus).ThenByDescending(o => o.Sort).ToList();
                    break;

                case "sortApplyStatus/desc":
                    data = data.OrderByDescending(o => o.ApplyStatus).ThenByDescending(o => o.Sort).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.PublishDateString).ThenByDescending(g => g.Sort).ToList();
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

            #endregion 驗證報名日期

            #region 驗證報名組數

            bool validApplyGroup = true;
            int groupLimitCount = DB.OLACTGROUP.Where(o => o.MAP_ACT_ID == id).Sum(s => s.TEAM_APPLY_LIMIT);
            int currentApplyGroup = DB.APPLY.Where(g => g.MAP_ACT_ID == id).Count();
            validApplyGroup = (groupLimitCount > currentApplyGroup);

            #endregion 驗證報名組數

            if (!validActApplyDatetime || !validApplyGroup)
            {
                valid = false;
            }

            return valid;
        }
    }
}