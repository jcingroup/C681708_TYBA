using Newtonsoft.Json;
using OutWeb.Entities;
using OutWeb.Models;
using OutWeb.Models.FrontEnd.ApplyModels;
using OutWeb.Models.Manage.ActivityModels;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OutWeb.Modules.FrontEnd
{
    public class ApplyFrontModule : IDisposable
    {
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        /// <summary>
        /// 新增一個報名編號
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string CreateApplyNumberByActivityID(int id, int applyId, DbContext dbContext)
        {
            int applyNewNumber = 0;
            string applyNewNum = string.Empty;
            var data = (dbContext as TYBADB).OLACT.Where(o => o.ID == id).FirstOrDefault();
            if (data == null)
                throw new Exception("無法取得該活動，是否已刪除?");
            string actNum = data.ACT_NUM;
            var applyNum = (dbContext as TYBADB).APPLY_NUMBER.Where(o => o.REF_ACT_ID == id).ToList();
            if (applyNum.Count > 0)
            {
                applyNewNumber = (applyNum.Select(s => s.APPLY_NUM).Max() + 1);
            }
            else
            {
                applyNewNumber = 1;
            }



            APPLY_NUMBER apNum = new APPLY_NUMBER()
            {
                REF_ACT_ID = id,
                MAP_APPLY_ID = applyId,
                APPLY_NUM = applyNewNumber
            };
            try
            {
                (dbContext as TYBADB).APPLY_NUMBER.Add(apNum);
                (dbContext as TYBADB).SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            applyNewNum = applyNewNumber.ToString().PadLeft(3, '0');
            return string.Concat(actNum, "-", applyNewNum);
        }

        /// <summary>
        /// 取得報名編號
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetApplyNumberByID(int id)
        {
            string applyNewNum = string.Empty;
            var applyNum = DB.APPLY_NUMBER.Where(o => o.MAP_APPLY_ID == id).FirstOrDefault();
            int actId = applyNum.REF_ACT_ID;
            var act = DB.OLACT.Where(o => o.ID == actId).FirstOrDefault();
            if (act == null)
                throw new Exception("無法取得該活動，是否已刪除?");
            string actNum = act.ACT_NUM;
            applyNewNum = applyNum.APPLY_NUM.ToString().PadLeft(3, '0');
            return string.Concat(actNum, "-", applyNewNum);
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

        /// <summary>
        /// 儲存報名資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ApplyDataModel SaveApply(ApplyDataModel model)
        {
            int identityId = 0;
            ApplyDataModel result = new ApplyDataModel();

            using (var transction = DB.Database.CurrentTransaction ?? DB.Database.BeginTransaction())
            {
                APPLY apply = new APPLY()
                {
                    MAP_ACT_GUP_ID = model.ApplyGroupID,
                    TEAM_COACH = model.Coach,
                    CONTACT = model.Contact,
                    CONTACT_PHONE = model.ContactPhone,
                    EMAIL = model.Email,
                    REMRK = model.Remark,
                    TEAM_NM = model.TeamName,
                    MAP_ACT_ID = model.ActivityID,
                };

                try
                {
                    DB.APPLY.Add(apply);
                    DB.SaveChanges();
                    identityId = apply.ID;
                    transction.Commit();
                }
                catch (Exception ex)
                {
                    transction.Rollback();
                    throw ex;
                }
            }

            using (var transaction = DB.Database.CurrentTransaction ?? DB.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                var aply = DB.APPLY.Where(o => o.ID == identityId).FirstOrDefault();
                aply.APPLY_IDEN_NUM = CreateApplyNumberByActivityID(model.ActivityID, identityId, DB);
                this.DB.Entry(aply).State = EntityState.Modified;
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
            };

            if (model.Member.Count > 0)
            {
                using (var transaction = DB.Database.CurrentTransaction ?? DB.Database.BeginTransaction())
                {
                    List<APPLY_MEMBER> memberList = new List<APPLY_MEMBER>();
                    foreach (var member in model.Member)
                    {
                        APPLY_MEMBER temp = new APPLY_MEMBER()
                        {
                            MEMBER_IDEN_ID = member.MemberIdentityID,
                            MEMBER_BIRDT = member.MemberBirthday,
                            MERBER_NM = member.MemberName,
                            MERBER_TP = member.MemberType,
                            BUD_DT = DateTime.UtcNow.AddHours(8),
                            MAP_APPLY_ID = identityId,
                            REF_ACT_ID = model.ActivityID
                        };
                        memberList.Add(temp);
                    };
                    try
                    {
                        DB.APPLY_MEMBER.AddRange(memberList);

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

            result = GetApplyDetailsByID(identityId, model.ActivityID);
            return result;
        }

        /// <summary>
        /// 取得報名明細
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actId"></param>
        /// <returns></returns>
        public ApplyDataModel GetApplyDetailsByID(int id, int actId)
        {
            ApplyDataModel model = new ApplyDataModel();
            var data = DB.APPLY.Where(o => o.ID == id && o.MAP_ACT_ID == actId).FirstOrDefault();
            if (data == null)
                throw new Exception("無法取得該報名檔，是否已刪除");
            model.ActivityTitle = DB.OLACT.Where(o => o.ID == actId).First().ACTITLE;
            model.ApplyGroupID = data.MAP_ACT_GUP_ID;
            model.ApplyGroupName = DB.OLACTGROUP.Where(o => o.MAP_ACT_ID == actId && o.ID == model.ApplyGroupID).First().GROUP_NAME;
            model.Coach = data.TEAM_COACH;
            model.Contact = data.CONTACT;
            model.ContactPhone = data.CONTACT_PHONE;
            model.Email = data.EMAIL;
            model.Remark = data.REMRK;
            model.TeamName = data.TEAM_NM;
            model.ApplyNumber = GetApplyNumberByID(id);
            var members = DB.APPLY_MEMBER.Where(o => o.MAP_APPLY_ID == id && o.REF_ACT_ID == actId).ToList();
            if (members.Count > 0)
            {
                List<MemberInfo> memberList = new List<MemberInfo>();
                foreach (var member in members)
                {
                    MemberInfo temp = new MemberInfo()
                    {
                        MemberIdentityID = member.MEMBER_IDEN_ID,
                        MemberBirthday = member.MEMBER_BIRDT,
                        MemberName = member.MERBER_NM,
                        MemberType = member.MERBER_TP
                    };
                    memberList.Add(temp);
                };
                model.Member.AddRange(memberList);
            }

            return model;
        }

        /// <summary>
        /// 取得報名明細 其中參賽群組已判斷報名數量
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ApplyViewDetailsModel GetViewActivityDetailsByID(int ID)
        {
            ApplyViewDetailsModel details =
                DB.OLACT.Where(w => w.ID == ID)
                .Select(s => new ApplyViewDetailsModel()
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
                var group = DB.OLACTGROUP
                                .Where(o => o.MAP_ACT_ID == ID)
                                .Select(s => new ApplyViewGroup()
                                {
                                    GroupID = s.ID,
                                    GroupName = s.GROUP_NAME,
                                    GroupApplyLimit = s.TEAM_APPLY_LIMIT,
                                    CountApplyLimit = s.COUNT_APPLY_LIMIT
                                })
                                .ToList();

                foreach (var g in group)
                {
                    var applyGorupLimitCount = DB.APPLY.Where(o => o.MAP_ACT_ID == ID && o.MAP_ACT_GUP_ID == g.GroupID).Count();
                    g.CountApplyLastLimit = (g.GroupApplyLimit - applyGorupLimitCount) <= 0 ? 0 : (g.GroupApplyLimit - applyGorupLimitCount);
                    details.ActivityGroup.Add(g);
                }
                details.ApplyGroupJsonString = JsonConvert.SerializeObject(details.ActivityGroup);
            }
            return details;
        }

        public ApplyListViewModel GetList(int page, bool getContent = false)
        {
            ApplyListViewModel model = new ApplyListViewModel();
            var data = DB.OLACT
                .AsEnumerable()
                .OrderByDescending(o => o.PUB_DT_STR).ThenByDescending(a => a.SQ)
                .Select(s => new ApplyListViewDataModel()
                {
                    ID = s.ID,
                    ActivityDateTimeDescription = s.ACT_DATE_DESC,
                    ApplyDateRange = string.Concat(s.APPLY_DATE_BEGIN, "~", s.APPLY_DATE_END),
                    Sort = s.SQ,
                    Title = s.ACTITLE,
                    Remark = s.ACT_CONTENT
                })
                .ToList();

            using (var actModule = new ActivityModule())
            {
                foreach (var d in data)
                {
                    d.GroupApplyLimit = DB.OLACTGROUP.Where(o => o.MAP_ACT_ID == d.ID).First().TEAM_APPLY_LIMIT;
                    d.Registered = DB.APPLY.Where(g => g.MAP_ACT_ID == d.ID).Count();
                    d.ActivityStatus = actModule.ActivityStatusCheckByID((int)d.ID);
                }
            }
            model.ListData = data;
            foreach (var d in data)
                PublicMethodRepository.HtmlDecode(d);

            if (!getContent)
                model = this.ListPagination(ref model, page, Convert.ToInt32(PublicMethodRepository.GetConfigAppSetting("DefaultPageSize")));
            return model;
        }
        /// <summary>
        /// [前台] 列表分頁處理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ApplyListViewModel ListPagination(ref ApplyListViewModel model, int page, int pageSize)
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
                    DataCount = model.ListData.Count(),
                    PageSize = pageSize,
                    FirstPage = 1,
                    LastPage = model.ListData.Count() == 0 ? 1 : Convert.ToInt32(Math.Ceiling((decimal)model.ListData.Count() / pageSize))
                };
            }
            model.ListData = model.ListData.Skip(startRow).Take(pageSize).ToList();
            model.Pagination = paginationResult;
            return model;
        }
    }
}