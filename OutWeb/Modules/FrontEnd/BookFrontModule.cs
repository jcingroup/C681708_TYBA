using OutWeb.Entities;
using OutWeb.Models.FrontEnd.BookFrontEndModels;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutWeb.Modules.FrontEnd
{
    public class BookFrontModule : IDisposable
    {
        private DBEnergy DB = new DBEnergy();

        public 分類明細檔 GetDefualtType()
        {
            var item = this.DB.功能項目檔.Where(o => o.項目代碼.StartsWith("book")).FirstOrDefault();
            if (item == null)
                throw new Exception("無法取得出版品分類");
            var map = this.DB.分類對應檔.Where(o => o.對應項目索引 == item.主索引).First();
            var type = this.DB.分類明細檔.Where(o => o.對應分類主檔索引 == map.對應分類類別索引).FirstOrDefault();
            if (type == null)
                throw new Exception("尚未建立分類");
            return type;
        }

        public 分類明細檔 GetTypeByID(int ID)
        {
            var item = this.DB.功能項目檔.Where(o => o.項目代碼.StartsWith("book")).FirstOrDefault();
            if (item == null)
                throw new Exception("無法取得出版品分類");
            var map = this.DB.分類對應檔.Where(o => o.對應項目索引 == item.主索引).First();
            var type = this.DB.分類明細檔.Where(o => o.對應分類主檔索引 == map.對應分類類別索引).Where(o => o.主索引 == ID).FirstOrDefault();
            if (type == null)
                throw new Exception("尚未建立分類");
            return type;
        }

        public List<BookFrontEndDataModel> GetList(BookFrontFilterModel filter)
        {
            PublicMethodRepository.FilterXss(filter);

            List<BookFrontEndDataModel> data = new List<BookFrontEndDataModel>();
            try
            {
                data = DB.出版品主檔
                    .ToList()
                    .Select(o => new BookFrontEndDataModel()
                    {
                        ID = o.主索引,
                        Title = o.名稱,
                        PublishDate = o.發稿時間,
                        Status = (bool)o.顯示狀態,
                        SortIndex = o.排序,
                        Type = (int)o.對應分類索引
                    })
                    .Where(o => o.Status == true)
                    .ToList();

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filter.QueryString))
                {
                    this.ListFilterQueryString(filter.QueryString, ref data);
                }
                //發佈日期搜尋
                this.ListDateFilter(filter.PublishBeginDate, filter.PublishEndDate, ref data);


                //分類
                if (!string.IsNullOrEmpty(filter.Type))
                {
                    this.ListTypeFilter(filter.Type, ref data);
                }

                data = data.OrderByDescending(o => o.SortIndex).ThenByDescending(o => o.PublishDate).ToList();

                //取圖檔
                foreach (var d in data)
                {
                    PublicMethodRepository.HtmlDecode(d);
                    ImgModule imgModule = new ImgModule();
                    d.CoverImg = imgModule.GetImages((int)d.ID, "Book", "M");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }

        /// <summary>
        /// 列表分類搜尋
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="data"></param>

        private void ListTypeFilter(string typeCode, ref List<BookFrontEndDataModel> data)
        {
            data = data.Where(s => s.Type.ToString().Contains(typeCode)).ToList();
        }
        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListDateFilter(DateTime publishBegindate, DateTime publishEnddate, ref List<BookFrontEndDataModel> data)
        {
            var r = data.Where(s => s.PublishDate >= publishBegindate && s.PublishDate <= publishEnddate).ToList();
            data = r;
        }

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilterQueryString(string filterStr, ref List<BookFrontEndDataModel> data)
        {
            var r = data.Where(s => s.Title.Contains(filterStr)).ToList();
            data = r;
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