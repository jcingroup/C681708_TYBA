using OutWeb.Entities;
using OutWeb.Models.FrontEnd.CaseFrontEndModels;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutWeb.Modules.FrontEnd
{
    public class CaseFrontModule : IDisposable
    {
        private DBEnergy DB = new DBEnergy();

        public int GetDefualtTypeID()
        {
            var item = this.DB.功能項目檔.Where(o => o.項目代碼.ToUpper().StartsWith("NEWS")).FirstOrDefault();
            if (item == null)
                throw new Exception("無法取得出版品分類");
            var map = this.DB.分類對應檔.Where(o => o.對應項目索引 == item.主索引).First();
            var type = this.DB.分類明細檔.Where(o => o.對應分類主檔索引 == map.對應分類類別索引).FirstOrDefault();
            if (type == null)
                throw new Exception("尚未建立分類");
            return type.主索引;
        }
        public List<CaseFrontListDataModel> GetListForFront()
        {
            List<CaseFrontListDataModel> result = this.DB.能源案例
                                   .ToList()
                                   .OrderByDescending(o => o.排序).ThenBy(a => a.建立日期)
                                   .Where(o => o.顯示狀態 == true && o.首頁顯示)
                                   .Select(o => new CaseFrontListDataModel()
                                   {
                                       ID = o.主索引,
                                       Title = o.案例標題,
                                       Type = o.設備別,
                                       TypeName = GetCasesTypeNameByID(o.設備別)
                                   })
                                   .Take(6)
                                 .ToList();

            foreach (var r in result)
                PublicMethodRepository.HtmlDecode(r);
            return result;
        }


        public Dictionary<string, List<CaseFrontListDataModel>> GetList()
        {
            Dictionary<string, List<CaseFrontListDataModel>> result = new Dictionary<string, List<CaseFrontListDataModel>>();
            try
            {
                var data = this.DB.能源案例
                                    .ToList()
                                    .OrderByDescending(o => o.排序).ThenBy(a => a.建立日期)
                                    .Where(o => o.顯示狀態 == true && o.首頁顯示)
                                    .Select(o => new CaseFrontListDataModel()
                                    {
                                        ID = o.主索引,
                                        Title = o.案例標題,
                                        Type = o.設備別,
                                        TypeName = GetCasesTypeNameByID(o.設備別)
                                    });

                if (data.Count() > 0)
                {
                    IEnumerable<IGrouping<string, CaseFrontListDataModel>> query =
                                  data.GroupBy(x => x.TypeName);
                    foreach (IGrouping<string, CaseFrontListDataModel> group in query)
                    {
                        result[group.Key] = new List<CaseFrontListDataModel>();
                        foreach (var item in group)
                        {
                            result[group.Key].Add(new CaseFrontListDataModel()
                            {
                                ID = item.ID,
                                Title = item.Title,
                                Type = item.Type,
                                TypeName = item.TypeName
                            });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            foreach (var r in result)
            {
                foreach (var item in r.Value as List<CaseFrontListDataModel>)
                    PublicMethodRepository.HtmlDecode(item);
            }

            return result;
        }
        /// <summary>
        /// 取得分類名稱
        /// </summary>
        /// <param name="typeID"></param>
        /// <returns></returns>
        private string GetCasesTypeNameByID(int typeID)
        {
            int casesTypeID = GetCaseTypeID();

            var type = this.DB.分類明細檔.Where(o => o.對應分類主檔索引 == casesTypeID && o.主索引 == typeID).FirstOrDefault();
            if (type == null)
                throw new Exception("無法取得節能案例分類檔");
            PublicMethodRepository.HtmlDecode(new List<string> { type.分類名稱 });
            return type.分類名稱;
        }

        /// <summary>
        /// 取得出版品在分類管理中的代碼
        /// </summary>
        /// <returns></returns>
        public int GetCaseTypeID()
        {
            int bookTypeID = 0;
            try
            {
                bookTypeID =
                       this.DB.功能項目檔.Join(this.DB.分類對應檔,
                       t1 => t1.主索引,
                       t2 => t2.對應項目索引,
                       (item, map) => new { Item = item, Map = map })
                       .Where(o => o.Item.項目代碼 == "Cases")
                       .First().Map.對應分類類別索引;
            }
            catch
            {
                throw new Exception("無法取得節能案例分類檔");
            }
            return bookTypeID;
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