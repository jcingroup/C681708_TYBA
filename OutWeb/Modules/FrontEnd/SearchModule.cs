using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.FrontEnd.SearchSiteModels;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OutWeb.Modules.FrontEnd
{
    public class SearchModule : IDisposable
    {
        private DBEnergy DB = new DBEnergy();

        public SearchListResultModel SearchSite(SearchListFilterModel filter)
        {
            PublicMethodRepository.FilterXss(filter);

            string str = filter.QueryString;
            SearchListResultModel result = new SearchListResultModel();
            List<SearchListDataModel> data = new List<SearchListDataModel>();

            var requestContext = new UrlHelper(System.Web.HttpContext.Current.Request.RequestContext);

            data.AddRange(DB.新聞
                    .Where(o => o.標題.Contains(str) ||
                    o.內容.Contains(str))
                    .Where(a => a.顯示狀態 == true)
                    .ToList()
                    .Select(o => new SearchListDataModel()
                    {
                        ID = o.主索引,
                        Content = o.內容,
                        Title = o.標題,
                        LinkAddr = string.Format("/News/Content?ID={0}&type={1}", o.主索引, o.分類代碼),
                        UpDateTime = o.修改日期,
                        Type = o.分類代碼
                    })
                    .ToList());

            data.AddRange(DB.課程
            .Where(o => o.標題.Contains(str) ||
            o.內容.Contains(str))
            .Where(a => a.顯示狀態 == true)
            .ToList()
            .Select(o => new SearchListDataModel()
            {
                ID = o.主索引,
                Content = o.內容,
                Title = o.標題,
                LinkAddr = requestContext.Action("Content", "Course", new { ID = o.主索引 }).ToString(),
                UpDateTime = o.修改日期
            })
            .ToList());

            data.AddRange(DB.問卷主檔
            .Where(o => o.問卷標題.Contains(str) ||
            o.問卷描述.Contains(str))
            .Where(a => a.是否上架 == true)
            .ToList()
            .Select(o => new SearchListDataModel()
            {
                ID = o.主索引,
                Content = o.問卷描述,
                Title = o.問卷標題,
                LinkAddr = requestContext.Action("Content", "Question", new { ID = o.主索引 }).ToString(),
                UpDateTime = o.更新日期
            })
            .ToList());

            data.AddRange(DB.能源案例
            .Where(o => o.案例標題.Contains(str) ||
            o.內容.Contains(str))
            .Where(a => a.顯示狀態 == true)
            .ToList()
            .Select(o => new SearchListDataModel()
            {
                ID = o.主索引,
                Content = o.內容,
                Title = o.案例標題,
                LinkAddr = requestContext.Action("Content", "Case", new { ID = o.主索引 }).ToString(),
                UpDateTime = o.更新日期
            })
            .ToList());

            data.AddRange(DB.研討會主檔
            .Join(
                this.DB.研討會明細檔,
                main => main.主索引,
                details => details.對應研討會主索引,
                (main, details) => new { Main = main, Details = details })
                .Where(w => w.Main.研討會名稱.Contains(str) || w.Details.活動內容.Contains(str))
            .Where(a => a.Details.顯示狀態 == true)
            .ToList()
            .Select(o => new SearchListDataModel()
            {
                ID = o.Main.主索引,
                Content = o.Details.活動內容,
                Title = o.Main.研討會名稱,
                LinkAddr = requestContext.Action("Content", "Train", new { trainID = o.Main.主索引 }).ToString(),
                UpDateTime = o.Main.更新日期
            })
            .ToList());

            data.AddRange(DB.出版品主檔
              .Where(o => o.名稱.Contains(str))
            .Where(a => a.顯示狀態 == true)
              .ToList()
              .Select(o => new SearchListDataModel()
              {
                  ID = o.主索引,
                  Content = o.名稱,
                  Title = o.摘要,
                  LinkAddr = requestContext.Action("Content", "Book", new { ID = o.主索引 }).ToString(),
                  UpDateTime = o.更新時間
              })
              .ToList());
            result.Data = data.OrderByDescending(o => o.UpDateTime).ToList();

            result = ListPagination(result, (int)filter.CurrentPage, (int)PageSizeConfig.SIZE30);
            foreach (var d in result.Data)
                PublicMethodRepository.HtmlDecode(d);
            return result;
        }

        public SearchListResultModel ListPagination(SearchListResultModel model, int page, int pageSize)
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