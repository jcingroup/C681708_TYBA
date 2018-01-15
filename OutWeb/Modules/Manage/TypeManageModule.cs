using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Exceptions;
using OutWeb.Models;
using OutWeb.Models.Manage.TypeManageModels;
using OutWeb.Provider;
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
    public class TypeManageModule : ListModuleService
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public override void DoDeleteByID(int ID)
        {
            //取隸屬分類的產品數量
            int typeId = ID;
            //var scCount = this.DB.分類對應檔.Where(o => o.主索引 == typeId).Count();
            //if (scCount > 0)
            //    throw new TypeManageRelationExcption("「尚有項目被歸類在此分類，故無法刪除。」");

            var data = this.DB.分類主檔.Where(s => s.主索引 == ID).FirstOrDefault();

            if (data == null)
                throw new Exception("[刪除分類類別] 查無此分類類別，可能已被移除");

            var map = this.DB.分類主檔.Where(s => s.主索引 == data.主索引).FirstOrDefault();
            var details = this.DB.分類明細檔.Where(s => s.對應分類主檔索引 == map.主索引).ToList();

            try
            {
                this.DB.分類明細檔.RemoveRange(details);
                this.DB.分類主檔.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除分類類別]" + ex.Message);
            }
        }

        public void DoDeleteSubDataByID(int ID, int filterTypeID)
        {
            //取隸屬分類的產品數量
            int typeId = ID;
            //var scCount = this.DB.分類對應檔.Where(o => o.主索引 == typeId).Count();
            //if (scCount > 0)
            //    throw new TypeManageRelationExcption("「尚有項目被歸類在此分類，故無法刪除。」");

            var data = this.DB.分類明細檔.Where(s => s.主索引 == ID && s.對應分類主檔索引 == filterTypeID).FirstOrDefault();

            if (data == null)
                throw new Exception("[刪除分類] 查無此分類類別，可能已被移除");

            try
            {
                this.DB.分類明細檔.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除分類]" + ex.Message);
            }
        }

        public override object DoGetDetailsByID(int ID)
        {
            TypeManageDetailsDataModel details =
              this.DB.分類主檔.Select(s => new TypeManageDetailsDataModel()
              {
                  ID = s.主索引,
                  Sort = s.排序,
                  TypeName = s.分類名稱,
                  Disabled = s.停用,
                  Remark = s.備註
              })
                .Where(o => o.ID == ID)
                .FirstOrDefault();

            if (details == null)
                throw new Exception("取得分類出現錯誤");

            #region 取該分類下的分類清單

            var subTypeList =
            this.DB.分類主檔.Join(this.DB.分類明細檔,
              t1 => t1.主索引,
              t2 => t2.對應分類主檔索引,
              (typeMain, typeDetails) => new TypeManageDetailsDataModel()
              {
                  ID = typeDetails.主索引,
                  Sort = typeDetails.排序,
                  TypeName = typeDetails.分類名稱,
                  Disabled = typeDetails.停用,
                  MappingID = typeDetails.對應分類主檔索引,
                  CustomID = typeDetails.自訂代碼
              })
              .Where(o => o.MappingID == ID)
              .ToList();

            #endregion 取該分類下的分類清單

            if (subTypeList.Count > 0)
                details.SubTypeList.AddRange(subTypeList);
            PublicMethodRepository.HtmlDecode(details);
            foreach (var sub in details.SubTypeList)
                PublicMethodRepository.HtmlDecode(sub);

            return details;
        }

        public TypeManageDetailsDataModel DoGetSubDetailsByID(int ID, int filterTypeID)
        {
            TypeManageDetailsDataModel details =
              this.DB.分類主檔.Join(this.DB.分類明細檔,
                t1 => t1.主索引,
                t2 => t2.對應分類主檔索引,
                (typeMain, typeDetails) => new TypeManageDetailsDataModel()
                {
                    ID = typeDetails.主索引,
                    Sort = typeDetails.排序,
                    TypeName = typeDetails.分類名稱,
                    Disabled = typeDetails.停用,
                    CustomID = typeDetails.自訂代碼,
                    MappingID = typeMain.主索引
                })
                .Where(o => o.MappingID == filterTypeID && o.ID == ID)
                .FirstOrDefault();
            PublicMethodRepository.HtmlDecode(details);
            if (details == null)
                throw new Exception("取得分類出現錯誤");

            return details;
        }

        public override object DoGetList<TFilter>(TFilter model, Language language)
        {
            TypeManageListFilterModel filterModel = (model as TypeManageListFilterModel);
            PublicMethodRepository.FilterXss(filterModel);
            TypeManageListResultModel result = new TypeManageListResultModel();

            List<TypeManageListDataModel> data = new List<TypeManageListDataModel>();

            try
            {
                #region 分類清單 有ID取分類底下清單

                if (filterModel.TypeID.HasValue)
                {
                    data = this.DB.分類明細檔.Select(s => new TypeManageListDataModel()
                    {
                        ID = s.主索引,
                        Sort = s.排序,
                        TypeName = s.分類名稱,
                        Disabled = s.停用,
                        MappingID = s.對應分類主檔索引
                    })
                                          .Where(o => o.MappingID == filterModel.TypeID)
                                          .OrderByDescending(o => o.Sort).ToList();
                }
                else
                {
                    data = this.DB.分類對應檔.ToList()
                        .Select(s => new TypeManageListDataModel()
                        {
                            ID = s.對應分類類別索引,
                            CreateDate = this.DB.分類主檔.Where(o => o.主索引 == s.對應分類類別索引).First().建立日期,
                            TypeName = this.DB.分類主檔.Where(o => o.主索引 == s.對應分類類別索引).First().分類名稱,
                            Sort = this.DB.分類主檔.Where(o => o.主索引 == s.對應分類類別索引).First().排序,
                            Disabled = this.DB.分類主檔.Where(o => o.主索引 == s.對應分類類別索引).First().停用,
                            MappingID = s.對應項目索引,
                            MappingItemName = this.DB.功能項目檔.Where(o => o.主索引 == s.對應項目索引).First().項目名稱
                        }).OrderByDescending(o => o.Sort).ToList();
                    //data = this.DB.分類主檔.Select(s => new TypeManageListDataModel()
                    //{
                    //    ID = s.主索引,
                    //    CreateDate = s.建立日期,
                    //    Sort = s.排序,
                    //    TypeName = s.分類名稱,
                    //    Disabled = s.停用
                    //}).OrderByDescending(o => o.Sort).ToList();
                }

                #endregion 分類清單 有ID取分類底下清單

                //語系搜尋
                //if (!language.Equals(Language.NotSet))
                //{
                //    this.ListFilterLanguage(language, ref data);
                //}

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }

                //狀態搜尋
                if (!string.IsNullOrEmpty(filterModel.Status))
                {
                    this.ListStatusFilter(filterModel.Status, ref data);
                }

                //取隸屬分類的產品數量
                //foreach (var d in data)
                //{
                //    int typeId = d.分類代碼;
                //    var scCount = this.DB.新聞.Where(o => o.分類代碼 == typeId).Count();
                //    d.ProductCount = scCount;
                //}

                //排序
                this.ListSort(filterModel.SortColumn, filterModel.Status, ref data);
                PaginationResult pagination;
                //分頁
                this.ListPageList(filterModel.CurrentPage, ref data, out pagination);
                result.Pagination = pagination;
                //if (!filterModel.TypeID.HasValue)
                //{
                //    foreach (var d in data)
                //    {
                //        var item = this.DB.分類對應檔.Where(o => o.對應分類類別索引 == d.ID).FirstOrDefault();
                //        if (item == null)
                //            continue;
                //        d.MappingItemName = this.DB.功能項目檔.Where(o => o.主索引 == item.對應項目索引).First().項目名稱;
                //    }
                //}

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

        public override int DoSaveData(FormCollection form, Language language, int? ID = default(int?), List<HttpPostedFileBase> image = null, List<HttpPostedFileBase> images = null)
        {
            分類主檔 saveModel;
            if (!ID.HasValue)
            {
                saveModel = new 分類主檔();
                saveModel.建立人員 = UserProvider.Instance.User.ID;
                saveModel.建立日期 = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.分類主檔.Where(s => s.主索引 == ID).FirstOrDefault();
            }
            saveModel.分類名稱 = form["typeName"] ?? "";
            saveModel.備註 = form["remark"] ?? "";
            saveModel.排序 = form["sortIndex"] == null ? 1 : form["sortIndex"] == string.Empty ? 1 : Convert.ToInt32(form["sortIndex"]);
            saveModel.停用 = form["disable"] == null ? false : true;
            saveModel.修改日期 = DateTime.UtcNow.AddHours(8);
            saveModel.修改人員 = UserProvider.Instance.User.ID;
            PublicMethodRepository.FilterXss(saveModel);
            if (ID.HasValue)
            {
                this.DB.Entry(saveModel).State = EntityState.Modified;
            }
            else
            {
                this.DB.分類主檔.Add(saveModel);
            }
            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            int identityId = saveModel.主索引;
            return identityId;
        }

        public int DoSaveSubTypeData(FormCollection form, Language language, int? ID = default(int?))
        {
            分類明細檔 saveMapDetailsModel;
            int filterTypeID = Convert.ToInt32(form["FilterTypeID"]);
            if (!ID.HasValue)
            {
                saveMapDetailsModel = new 分類明細檔();
                saveMapDetailsModel.對應分類主檔索引 = filterTypeID;
            }
            else
            {
                saveMapDetailsModel = this.DB.分類明細檔.Where(s => s.主索引 == ID && s.對應分類主檔索引 == filterTypeID).FirstOrDefault();
            }
            saveMapDetailsModel.分類名稱 = form["typeName"];
            saveMapDetailsModel.排序 = form["sortIndex"] == null ? 1 : form["sortIndex"] == string.Empty ? 1 : Convert.ToInt32(form["sortIndex"]);
            saveMapDetailsModel.停用 = form["disable"] == null ? false : true;
            saveMapDetailsModel.自訂代碼 = Convert.ToInt32(form["customCode"]);
            PublicMethodRepository.FilterXss(saveMapDetailsModel);
            if (ID.HasValue)
            {
                this.DB.Entry(saveMapDetailsModel).State = EntityState.Modified;
            }
            else
            {
                this.DB.分類明細檔.Add(saveMapDetailsModel);
            }
            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            int identityId = saveMapDetailsModel.主索引;
            return identityId;
        }

        /// <summary>
        /// 語系過濾條件
        /// </summary>
        /// <param name="language"></param>
        /// <param name="data"></param>
        //private void ListFilterLanguage(Language language, ref List<一般分類> data)
        //{
        //    var r = data.Where(s => s.Language == language.GetCode()).ToList();
        //    data = r;
        //}

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<TypeManageListDataModel> data)
        {
            var r = data.Where(s => s.TypeName.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        //private void ListDateFilter(string beginDate, string endDate, ref List<ProductKindListDataModel> data)
        //{
        //    var r = data.Where(s => Convert.ToDateTime(s.PublishDateStr) >= Convert.ToDateTime(beginDate) && Convert.ToDateTime(s.PublishDateStr) <= Convert.ToDateTime(endDate)).ToList();
        //    data = r;
        //}

        /// <summary>
        /// 狀態搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListStatusFilter(string filterStatus, ref List<TypeManageListDataModel> data)
        {
            bool bStatus = filterStatus == "Y" ? false : true;
            var r = data.Where(s => s.Disabled == bStatus).ToList();
            data = r;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<TypeManageListDataModel> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, string status, ref List<TypeManageListDataModel> data)
        {
            switch (sortCloumn)
            {
                case "sortName/asc":
                    data = data.OrderBy(o => o.TypeName.ToString()).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortName/desc":
                    data = data.OrderByDescending(o => o.TypeName.ToString()).ThenByDescending(g => g.Sort).ToList();
                    break;

                //case "sortStatus/asc":
                //    data = data.OrderBy(o => o.StatusCode).ThenByDescending(g => g.Sort).ToList();
                //    break;

                //case "sortStatus/desc":
                //    data = data.OrderByDescending(o => o.StatusCode).ThenByDescending(g => g.Sort).ToList();
                //    break;

                case "sortIndex/asc":
                    data = data.OrderBy(o => o.Sort).ThenByDescending(g => g.CreateDate).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.Sort).ThenByDescending(g => g.CreateDate).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.Sort).ToList();
                    break;
            }
        }

        /// <summary>
        /// 取得分類名稱
        /// </summary>
        /// <param name="typeID"></param>
        /// <returns></returns>
        public string GetTypeNameByTypeID(int typeID)
        {
            string typeName = this.DB.分類主檔.Where(o => o.主索引 == typeID).First().分類名稱;
            PublicMethodRepository.HtmlDecode(new List<string> { typeName });
            return typeName;
        }

        /// <summary>
        /// 取得產品分類下拉選單資料
        /// </summary>
        /// <param name="typeID">主分類代碼</param>
        /// <param name="isListMode">是否為列表模式</param>
        /// <param name="filterTypeID">分類類別代碼</param>
        /// <param name="filterTypeSQ">要取得的分類項次</param>
        /// <returns></returns>
        public SelectList CreateTypeManageDropList(int? typeID, bool isListMode = true, string filterTypeCode = null, int? filterTypeSQ = 1)
        {
            List<SelectListItem> types = null;
            SelectList typeList;

            if (!string.IsNullOrEmpty(filterTypeCode))
            {
                var itemObj = this.DB.功能項目檔.Where(o => o.項目代碼 == filterTypeCode).FirstOrDefault();
                if (itemObj == null)
                    throw new TypeIsNotCreateException();
                var findTypeList = this.DB.分類對應檔.Where(o => o.對應項目索引 == itemObj.主索引).ToList();
                if (findTypeList.Count == 0)
                    throw new TypeIsNotCreateException();
                var findType = findTypeList.Where(o => o.項次 == filterTypeSQ).First();
                var getType = DB.分類主檔
               .Where(o => o.停用 == false && o.主索引 == findType.對應分類類別索引).First();
                types =
                         DB.分類明細檔
                         .Where(o => o.停用 == false &&
                          o.對應分類主檔索引 == getType.主索引)
                         .Select(s => new SelectListItem() { Value = s.主索引.ToString(), Text = s.分類名稱 })
                         .ToList();
                if (types.Count == 0)
                    throw new TypeIsNotCreateException();
            }
            else
            {
                if (!typeID.HasValue)
                {
                    types =
                  DB.分類主檔
                  .Where(o => o.停用 == false)
                  .Select(s => new SelectListItem() { Value = s.主索引.ToString(), Text = s.分類名稱 })
                  .ToList();
                }
                else
                {
                    types =
                    DB.分類明細檔
                    .Where(o => o.停用 == false &&
                     o.對應分類主檔索引 == typeID)
                    .Select(s => new SelectListItem() { Value = s.主索引.ToString(), Text = s.分類名稱 })
                    .ToList();
                }
            }

            string itemTxt = "";
            if (isListMode)
                itemTxt = "全部";
            else
                itemTxt = "請選擇";
            if (typeID.HasValue)
            {
                types.Insert(0, new SelectListItem() { Value = "", Text = itemTxt });
                typeList = new SelectList(types, "Value", "Text", typeID.ToString());
            }
            else
            {
                types.Insert(0, new SelectListItem() { Value = "", Text = itemTxt, Selected = true });
                typeList = new SelectList(types, "Value", "Text", "0");
            }
            List<SelectListItem> resultItem = new List<SelectListItem>();
            SelectList resultSelectLis = null;
            foreach (var t in typeList)
                resultItem.Add(new SelectListItem() { Value = t.Value, Text = HttpUtility.HtmlDecode(t.Text) });
            resultSelectLis = new SelectList(resultItem, "Value", "Text", typeID.ToString());

            return resultSelectLis;
        }

        public SelectList GetItemDropDownList(bool isGetItems, int set = 0)
        {
            List<SelectListItem> types = null;
            SelectList typeList;
            if (isGetItems)
            {
                types = this.DB.功能項目檔
               .Select(s => new SelectListItem() { Value = s.主索引.ToString(), Text = s.項目名稱 })
               .ToList();
            }
            else
            {
                types = this.DB.分類主檔
               .Select(s => new SelectListItem() { Value = s.主索引.ToString(), Text = s.分類名稱 })
               .ToList();
            }
            types.Insert(0, new SelectListItem() { Value = "", Text = "請選擇" });
            typeList = new SelectList(types, "Value", "Text", set.ToString());
            List<SelectListItem> resultItem = new List<SelectListItem>();
            SelectList resultSelectLis = null;
            foreach (var t in typeList)
                resultItem.Add(new SelectListItem() { Value = t.Value, Text = HttpUtility.HtmlDecode(t.Text) });
            resultSelectLis = new SelectList(resultItem, "Value", "Text", set.ToString());

            return resultSelectLis;
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