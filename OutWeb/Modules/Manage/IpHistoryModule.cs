using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.IPModels;
using OutWeb.Provider;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OutWeb.Modules.Manage
{
    public class IpHistoryModule:IDisposable
    {
        private  DBEnergy DB = new DBEnergy();



        /// <summary>
        /// 取得目網站累積人數
        /// </summary>
        /// <param name="siteType"></param>
        /// <returns></returns>
        public int GetCurrentSiteCount(string siteType, string pam = null)
        {
            var current = DB.SiteCounter.Where(o => o.SITE_TYPE == siteType && o.PARM == pam).FirstOrDefault();
            if (current == null)
                return 0;
            else
                return current.SITE_COUNT;
        }

        /// <summary>
        /// 增加一筆瀏覽記數
        /// </summary>
        /// <param name="siteType"></param>
        public  void AddCount(string siteType, string pam = null)
        {
            SiteCounter save = null;
            var current = DB.SiteCounter.Where(o => o.SITE_TYPE == siteType && o.PARM== pam).FirstOrDefault();
            if (current == null)
                save = new SiteCounter() { SITE_TYPE = siteType };
            else
                save = current;

            ++save.SITE_COUNT;
            save.PARM = pam;
            if (current == null)
                DB.SiteCounter.Add(save);
            else
                DB.Entry(save).State = EntityState.Modified;
            DB.SaveChanges();
        }

        /// <summary>
        /// 累積人數處理函式
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="siteType"></param>
        public  void SiteCountHandler(string ip, string siteType)
        {
            DateTime currentTime = DateTime.UtcNow.AddHours(8);
            bool hasLogin = true;
            var chk = DB.SystemLog.Where(o => o.FromIP == ip && o.Parm == siteType).OrderByDescending(o => o.OperateDate).FirstOrDefault();
            if (chk == null)
                hasLogin = false;
            else
            {
                DateTime historyTime = chk.OperateDate;
                var timeSpan = new TimeSpan(currentTime.Ticks - historyTime.Ticks).TotalHours;
                if (timeSpan > 24)
                    hasLogin = false;
            }
            if (hasLogin == false)
            {
                WriteIp(ip, "front");
                AddCount(siteType);
            }
        }

        /// <summary>
        /// 寫入IP紀錄LOG檔
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="siteType"></param>
        public  void WriteIp(string ip, string siteType)
        {
            SystemLog log = new SystemLog()
            {
                OperateType = "L",
                OperateDate = DateTime.UtcNow.AddHours(8),
                FromIP = ip,
                OperateID = (siteType == "manager" ? UserProvider.Instance.User.ID.ToString() : ""),
                Parm = siteType
            };
            DB.SystemLog.Add(log);
            DB.SaveChanges();
        }

        /// <summary>
        /// 取得IP LOG List
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public  IPListResultModel GetList(IPListFilterModel filter)
        {
            IPListResultModel model = new IPListResultModel();
            var data = DB.SystemLog.Where(o => o.Parm == "manager").ToList();

            if (!string.IsNullOrEmpty(filter.QueryString))
            {
                data = data.Where(o => o.FromIP.Contains(filter.QueryString)).ToList();
            }

            data = data
                .Where(o => o.OperateDate >= filter.BeginDate && o.OperateDate <= filter.EndDate && o.Parm == "manager")
                .OrderBy(o => o.OperateDate)
                .ToList();

            PaginationResult pagination;
            ListPageList(filter.CurrentPage, ref data, out pagination);
            model.Pagination = pagination;
            model.Data = data;
            return model;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private  void ListPageList(int currentPage, ref List<SystemLog> data, out PaginationResult pagination)
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

   

        public void Dispose()
        {
            if (DB != null)
            {
                DB.Dispose();
                DB = null;
            }
        }
    }
}