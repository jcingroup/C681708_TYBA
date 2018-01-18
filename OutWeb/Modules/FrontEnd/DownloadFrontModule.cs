using OutWeb.Entities;
using OutWeb.Models;
using OutWeb.Models.FrontEnd.DownloadFrontModel;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.Linq;

namespace OutWeb.Modules.FrontEnd
{
    public class DownloadFrontModule : IDisposable
    {
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public DownloadFrontResultModel GetList(int page)
        {
            DownloadFrontResultModel result = new DownloadFrontResultModel();
            try
            {
                var data = this.DB.DLFILES
                                    .AsEnumerable()
                                    .OrderByDescending(o => o.PUB_DT_STR).ThenByDescending(a => a.SQ)
                                    .Where(o => o.DISABLE == false)
                                    .Select(o => new DownloadFrontDataModel()
                                    {
                                        ID = o.ID,
                                        Title = o.TITLE,
                                        PublishDateStr = o.PUB_DT_STR
                                    })
                                    .ToList();
                result.Data = data;
                result = this.ListPagination(ref result, page, Convert.ToInt32(PublicMethodRepository.GetConfigAppSetting("DefaultPageSize")));

                foreach (var item in data)
                    PublicMethodRepository.HtmlDecode(item);


                using (var fileModule = new FileModule())
                {
                    foreach (var item in data)
                        item.Files = fileModule.GetFiles((int)item.ID, "Download", "F");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// [前台] 列表分頁處理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public DownloadFrontResultModel ListPagination(ref DownloadFrontResultModel model, int page, int pageSize)
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