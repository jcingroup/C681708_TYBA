using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.ExportExcelModels.QuestionnaireStatisticsReplyModels;
using OutWeb.Models.Manage.QuestionnairesModels;
using OutWeb.Models.Manage.QuestionnaireStatisticsModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 問卷統計模組
    /// </summary>
    public class QuestionnaireStatisticsModule : IDisposable
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        //public MemoryStream GetReplyListByID(int ID)
        //{
        //    //取出要匯出Excel的資料
        //    List<問卷題目類型檔> rangerList = DB.問卷題目類型檔.ToList();

        //    //建立Excel
        //    ExcelPackage ep = new ExcelPackage();

        //    //建立第一個Sheet，後方為定義Sheet的名稱
        //    ExcelWorksheet sheet = ep.Workbook.Worksheets.Add("FirstSheet");
        //    //欄:直，因為要從第1欄開始，所以初始為1
        //    int col = 1;

        //    //標題列
        //    sheet.Cells[1, col++].Value = "ID";
        //    sheet.Cells[1, col++].Value = "Name";

        //    //資料列:橫
        //    int row = 2;
        //    foreach (var item in rangerList)
        //    {
        //        col = 1;//每換一列，欄位要從1開始 指定Sheet的欄與列(欄名列號ex.A1,B20，在這邊都是用數字)，將資料寫入
        //        sheet.Cells[row, col++].Value = item.主索引;
        //        sheet.Cells[row, col++].Value = item.問卷類型名稱;
        //        row++;
        //    }

        //    //資料流寫入
        //    MemoryStream fileStream = new MemoryStream();
        //    ep.SaveAs(fileStream);
        //    ep.Dispose();
        //    //不重新將位置設為0，excel開啟後會出現錯誤
        //    fileStream.Position = 0;
        //    return fileStream;
        //}

        public ReplyDataModel GetReplyDataByID(int ID)
        {
            ReplyDataModel model = new ReplyDataModel();
            var reply = this.DB.問卷答案主檔
                                .Join(
                                     this.DB.問卷答案明細檔,
                                    main => main.主索引,
                                    details => details.對應問卷答案索引,
                                    (main, details) => new { Main = main, Details = details })
                                    .ToList()
                                    .Where(o => o.Main.對應問卷主檔索引 == ID)
                                    .Select(o => new QuestionnairesReplyDetails()
                                    {
                                        TopicAnswerUser = o.Main.填寫人,
                                        TopicAnswerID = o.Main.填寫人ID,
                                        TopicID = o.Details.對應問卷題目索引,
                                        AnswerDate = o.Details.回答日期,
                                        TopicContent = GetTopicContentNameByID(o.Details.對應問卷題目索引),
                                        TopicTypeName = GetTopicTypeNameByID((int)o.Details.對應問題類型索引),
                                        TopicAnswerItemNumber = o.Details.問卷答案選項值,
                                        TopicAnswerTextContent = o.Details.問卷答案文字值
                                    })
                                    .ToList();

            reply.All(o =>
           {
               if (o.TopicAnswerItemNumber != null)
                   o.TopicAnswerItemContent = GetTopicItemNameByID((int)o.TopicAnswerItemNumber);
               return true;
           });
            model.QuestionnairesID = ID;
            model.QuestionnairesName = this.DB.問卷主檔.Where(o => o.主索引 == ID).First().問卷標題;
            model.Details = reply;

            PublicMethodRepository.HtmlDecode(model);
            foreach (var d in model.Details)
                PublicMethodRepository.HtmlDecode(d);
            return model;
        }

        /// <summary>
        /// 取得問卷題目內容
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private string GetTopicContentNameByID(int ID)
        {
            var result = this.DB.問卷題目檔.Where(o => o.主索引 == ID).First().問卷題目內容;
            PublicMethodRepository.HtmlDecode(new List<string> { result });
            return result;
        }

        /// <summary>
        /// 取得題目項目名稱內容
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private string GetTopicItemNameByID(int ID)
        {
            var result = this.DB.問卷選項檔.Where(o => o.主索引 == ID).First().題目選項內容;
            PublicMethodRepository.HtmlDecode(new List<string> { result });
            return result;
        }

        /// <summary>
        /// 取得題目類型名稱
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private string GetTopicTypeNameByID(int ID)
        {
            var result = this.DB.問卷題目類型檔.Where(o => o.主索引 == ID).First().問卷類型名稱;
            PublicMethodRepository.HtmlDecode(new List<string> { result });
            return result;
        }

        /// <summary>
        /// 取得一筆問卷統計
        /// </summary>
        /// <param name="ID">問卷主檔ID</param>
        /// <returns></returns>

        public QuestionnaireStatisticsDetailsDataModel DoGetDetailsByID(int ID)
        {
            var questMain = this.DB.問卷主檔.Where(o => o.主索引 == ID).FirstOrDefault();
            if (questMain == null)
                throw new Exception("無法取得該問卷");
            //var questMathMain = this.DB.問卷答案主檔.Where(o=>o.對應問卷主檔索引 ==).
            string currentUrl = "http://" + HttpContext.Current.Request.Url.Authority;
            QuestionnaireStatisticsDetailsDataModel result = new QuestionnaireStatisticsDetailsDataModel()
            {
                ID = questMain.主索引,
                Title = questMain.問卷標題,
                SendCount = questMain.預計發出量,
                RecoveredCount = GetReplyCountByOsnID(questMain.主索引),
                Responseate = MathResponseateByOsnID(questMain.主索引),
            };

            Func<int, string> getTopName = GetTopicStatisticNameByID;
            var questionList = this.DB.問卷題目檔.Where(o => o.對應問卷索引 == result.ID).ToList();
            foreach (var q in questionList)
            {
                TopicStatistic top = new TopicStatistic()
                {
                    ID = q.主索引,
                    Name = q.問卷題目內容,
                    TypeID = (int)q.對應問卷題目類型索引,
                    TypeName = getTopName((int)q.對應問卷題目類型索引),
                    ReplyCount = GetTopicReplyCountByOsnID(ID, q.主索引),
                    Responseate = MathResponseateByOsnID(ID, q.主索引),
                    Proportion = string.Concat(MathResponseateByOsnID(ID, q.主索引), "%")
                };

                var options = this.DB.問卷選項檔.Where(o => o.對應問卷題目索引 == q.主索引).ToList();

                foreach (var op in options)
                {
                    ReplyOptionModel reply = new ReplyOptionModel()
                    {
                        OptionID = op.主索引,
                        OptionName = op.題目選項內容,
                        ReplyCount = GetTopicOptionCountByOsnID(ID, op.對應問卷題目索引, op.主索引),
                        Responseate = MathOptionResponseateByOsnID(ID, op.對應問卷題目索引, op.主索引),
                        Proportion = string.Concat(MathOptionResponseateByOsnID(ID, op.對應問卷題目索引, op.主索引), "%")
                    };
                    top.ReplyList.Add(reply);
                }
                result.TopicStatistics.Add(top);
            }

            PublicMethodRepository.HtmlDecode(result);
            foreach (var topic in result.TopicStatistics)
            {
                PublicMethodRepository.HtmlDecode(topic);
                foreach (var reply in topic.ReplyList)
                    PublicMethodRepository.HtmlDecode(reply);
            }

            return result;
        }

        private string GetTopicStatisticNameByID(int ID)
        {
            var type = this.DB.問卷題目類型檔.Where(o => o.主索引 == ID).First();
            PublicMethodRepository.HtmlDecode(type);
            return type.問卷類型名稱;
        }

        public QuestionnaireStatisticsListResultModel DoGetList(QuestionnaireStatisticsListFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);
            QuestionnaireStatisticsListResultModel result = new QuestionnaireStatisticsListResultModel();
            List<QuestionnaireStatisticsListDataModel> data = new List<QuestionnaireStatisticsListDataModel>();
            try
            {
                data = DB.問卷主檔
                    .ToList()
                    .Select(o => new QuestionnaireStatisticsListDataModel()
                    {
                        ID = o.主索引,
                        Title = o.問卷標題,
                        ReplyCount = GetReplyCountByOsnID(o.主索引),
                        Responseate = MathResponseateByOsnID(o.主索引),
                        SendCount = o.預計發出量,
                        ValidDate = string.Concat(o.開放時間起始日.ToString("yyyy\\/MM\\/dd"), "~", o.開放時間結束日.ToString("yyyy\\/MM\\/dd"))
                    })
                    .ToList();

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }

                //排序
                this.ListSort(filterModel.SortColumn, filterModel.Status, ref data);
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

        /// <summary>
        /// 取得問卷回收量(為填寫數量)
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private int GetReplyCountByOsnID(int ID)
        {
            int reply = this.DB.問卷答案主檔.Where(o => o.對應問卷主檔索引 == ID).Count();
            return reply;
        }

        /// <summary>
        /// 計算問卷回覆率
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private double MathResponseateByOsnID(int ID)
        {
            double replyCount = this.DB.問卷答案主檔.Where(o => o.對應問卷主檔索引 == ID).Count();
            double sendCount = this.DB.問卷主檔.Where(o => o.主索引 == ID).First().預計發出量;
            double math = Math.Round(((replyCount / sendCount) * 100), 2);
            math = Double.IsNaN(math) ? 0.0 : math;
            return math;
        }

        /// <summary>
        /// 取得題目回收量(為填寫數量)
        /// </summary>
        /// <param name="qsnID">問卷ID</param>
        /// <param name="topID">題目ID</param>
        /// <returns></returns>
        private int GetTopicReplyCountByOsnID(int qsnID, int topID)
        {
            List<int> questMathMainIdList = this.DB.問卷答案主檔
                .Where(o => o.對應問卷主檔索引 == qsnID)
                .Select(s => s.主索引)
                .ToList();
            int replyCount = 0;
            var reply = this.DB.問卷答案明細檔.Where(o => questMathMainIdList.Contains(o.對應問卷答案索引)
                && o.對應問卷題目索引 == topID
                ).ToList();
            //避免複選題重複計算，以groupby分開
            IEnumerable<IGrouping<int, 問卷答案明細檔>> query =
                        reply.GroupBy(x => x.對應問卷答案索引);
            replyCount = query.Count();
            return replyCount;
        }

        /// <summary>
        /// 計算題目回覆率
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private double MathResponseateByOsnID(int qsnID, int topID)
        {
            //取得該題目回答總數量 為計算分母
            var replyMainCount = this.DB.問卷答案主檔
                .Where(o => o.對應問卷主檔索引 == qsnID).Count();

            //取得回覆此題的數量
            double replyCount = GetTopicReplyCountByOsnID(qsnID, topID);
            double math = Math.Round(((replyCount / replyMainCount) * 100), 2);
            math = Double.IsNaN(math) ? 0.0 : math;
            return math;
        }

        /// <summary>
        /// 取得項目回收量(為填寫數量)
        /// </summary>
        /// <param name="qsnID">問卷ID</param>
        /// <param name="topID">題目ID</param>
        /// <returns></returns>
        private int GetTopicOptionCountByOsnID(int qsnID, int topID, int optionID)
        {
            List<int> ansQuestIdList = this.DB.問卷答案主檔
                .Where(o => o.對應問卷主檔索引 == qsnID)
                .Select(s => s.主索引)
                .ToList();

            int reply = this.DB.問卷答案明細檔.Where(o => o.對應問卷題目索引 == topID
           && o.問卷答案選項值 == optionID).Count();

            return reply;
        }

        /// <summary>
        /// 計算項目回覆率
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private double MathOptionResponseateByOsnID(int qsnID, int topID, int optionID)
        {
            //取得該題目回答總數量 為計算分母
            var replyMainCount = this.DB.問卷答案主檔
                .Where(o => o.對應問卷主檔索引 == qsnID).Count();
            //取得回覆此項目的數量
            double replyCount = GetTopicOptionCountByOsnID(qsnID, topID, optionID);
            double math = Math.Round(((replyCount / replyMainCount) * 100), 2);
            math = Double.IsNaN(math) ? 0.0 : math;
            return math;
        }

        public int DoSaveData(QuestionDetailsDataModel saveModel)
        {
            問卷主檔 questMain;

            if (saveModel.ID == 0)
            {
                questMain = new 問卷主檔();
                questMain.建立人員 = UserProvider.Instance.User.ID;
                questMain.建立日期 = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                questMain = this.DB.問卷主檔.Where(s => s.主索引 == saveModel.ID).FirstOrDefault();
            }

            questMain.問卷標題 = saveModel.Title;
            questMain.問卷描述 = saveModel.Description;
            questMain.開放時間起始日 = Convert.ToDateTime(saveModel.OpeningTime);
            questMain.開放時間結束日 = Convert.ToDateTime(saveModel.EndTime);
            questMain.預計發出量 = saveModel.SendCount;
            questMain.排序 = saveModel.Sort;
            questMain.是否上架 = saveModel.Status == null ? false : true;
            questMain.是否需要登入 = saveModel.IsSignIn == null ? false : true; ;
            questMain.更新人員 = UserProvider.Instance.User.ID;
            questMain.更新日期 = DateTime.UtcNow.AddHours(8);
            PublicMethodRepository.FilterXss(questMain);
            if (saveModel.ID > 0)
            {
                this.DB.Entry(questMain).State = EntityState.Modified;
            }
            else
            {
                this.DB.問卷主檔.Add(questMain);
            }

            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            int identityId = (int)questMain.主索引;
            SaveQuestionSetting(identityId, saveModel.Data);
            return identityId;
        }

        private void SaveQuestionSetting(int identityId, QuestionDetailsData model)
        {
            PublicMethodRepository.FilterXss(model);
            foreach (var topic in model.Topic)
            {
                PublicMethodRepository.FilterXss(topic);
                foreach (var option in topic.Option)
                    PublicMethodRepository.FilterXss(option);
            }
            var QuestionnaireStatisticsList = this.DB.問卷題目檔.Where(o => o.對應問卷索引 == identityId).ToList();
            var optionList = this.DB.問卷選項檔.Where(o => o.對應問卷主檔索引 == identityId).ToList();
            if (QuestionnaireStatisticsList.Count > 0)
            {
                this.DB.問卷題目檔.RemoveRange(QuestionnaireStatisticsList);
                this.DB.SaveChanges();
            }
            if (optionList.Count > 0)
            {
                this.DB.問卷選項檔.RemoveRange(optionList);
                this.DB.SaveChanges();
            }

            foreach (var q in model.Topic)
            {
                問卷題目檔 questionSetting = new 問卷題目檔()
                {
                    對應問卷索引 = identityId,
                    問卷題目內容 = q.TopicContent,
                    排序 = q.Sort,
                    對應問卷題目類型索引 = q.TopicType,
                    是否必填 = q.Required == null ? false : true,
                    更新人員 = UserProvider.Instance.User.ID,
                    更新日期 = DateTime.UtcNow.AddHours(8)
                };
                this.DB.問卷題目檔.Add(questionSetting);
                this.DB.SaveChanges();

                foreach (var op in q.Option)
                {
                    問卷選項檔 optionSetting = new 問卷選項檔()
                    {
                        題目選項內容 = op.OptionValue,
                        對應問卷主檔索引 = identityId,
                        對應問卷題目索引 = questionSetting.主索引,
                        對應問題類型索引 = q.TopicType
                    };
                    this.DB.問卷選項檔.Add(optionSetting);
                    this.DB.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<QuestionnaireStatisticsListDataModel> data)
        {
            var r = data.Where(s => s.Title.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<QuestionnaireStatisticsListDataModel> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, string status, ref List<QuestionnaireStatisticsListDataModel> data)
        {
            switch (sortCloumn)
            {
                case "sortSend/asc":
                    data = data.OrderBy(o => o.SendCount).ToList();
                    break;

                case "sortSend/desc":
                    data = data.OrderByDescending(o => o.SendCount).ToList();
                    break;

                case "sortReplyCount/asc":
                    data = data.OrderBy(o => o.ReplyCount).ToList();
                    break;

                case "sortReplyCount/desc":
                    data = data.OrderByDescending(o => o.ReplyCount).ToList();
                    break;

                case "sortResponseeate/asc":
                    data = data.OrderBy(o => o.Responseate).ToList();
                    break;

                case "sortResponseeate/desc":
                    data = data.OrderByDescending(o => o.Responseate).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.SendCount).ToList();
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
    }
}