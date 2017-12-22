using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.FrontEnd.QuestionnairesModels;
using OutWeb.Models.Manage.QuestionnairesModels;
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
    /// 最新消息列表模組
    /// </summary>
    public class QuestionnairesModule : IDisposable
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public void DoDeleteByID(int ID)
        {
            var data = this.DB.問卷主檔.Where(s => s.主索引 == ID).FirstOrDefault();
            if (data == null)
                throw new Exception("[刪除問卷] 查無此問卷，可能已被移除");
            var hasAns = this.DB.問卷答案主檔.Where(o => o.對應問卷主檔索引 == ID).Count() > 0;

            #region 判斷是否有人填寫

            if (hasAns)
                throw new Exception("[刪除問卷] 無法刪除此問卷，已有填寫數量");

            #endregion 判斷是否有人填寫

            #region 刪除答案以及選項檔

            var optionList = this.DB.問卷選項檔.Where(o => o.對應問卷主檔索引 == ID).ToList();
            List<int> optionIDList = new List<int>();
            foreach (var op in optionList)
                optionIDList.Add(op.主索引);
            if (optionIDList.Count > 0)
            {
                this.DB.問卷選項檔.RemoveRange(this.DB.問卷選項檔.Where(o => optionIDList.Contains(o.主索引)).ToList());
            }

            #endregion 刪除答案以及選項檔

            #region 問卷題目檔

            this.DB.問卷題目檔.RemoveRange(this.DB.問卷題目檔.Where(o => o.對應問卷索引 == ID).ToList());

            #endregion 問卷題目檔

            #region 問卷主檔

            this.DB.問卷主檔.RemoveRange(this.DB.問卷主檔.Where(o => o.主索引 == ID).ToList());

            #endregion 問卷主檔

            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除問卷]" + ex.Message);
            }
        }

        public QuestionDetailsDataModel DoGetDetailsByID(int ID)
        {
            var questMain = this.DB.問卷主檔.Where(o => o.主索引 == ID).FirstOrDefault();
            if (questMain == null)
                throw new Exception("無法取得該問卷");
            QuestionDetailsDataModel result = new QuestionDetailsDataModel()
            {
                ID = questMain.主索引,
                Title = questMain.問卷標題,
                Description = questMain.問卷描述,
                OpeningTime = questMain.開放時間起始日.ToString("yyyy\\/MM\\/dd"),
                EndTime = questMain.開放時間結束日.ToString("yyyy\\/MM\\/dd"),
                SendCount = questMain.預計發出量,
                Sort = questMain.排序,
                Status = questMain.是否上架 ? "Y" : "N",
                IsSignIn = questMain.是否需要登入 ? "on" : "off",
                Url = questMain.問卷網址,
                CreateDate = questMain.建立日期
            };

            var questDetails = this.DB.問卷題目檔.Where(o => o.對應問卷索引 == ID).ToList();
            List<Topic> questions = new List<Topic>();
            foreach (var q in questDetails)
            {
                Topic qTmep = new Topic()
                {
                    ID = q.主索引,
                    TopicContent = q.問卷題目內容,
                    TopicType = (int)q.對應問卷題目類型索引,
                    TopicTypeName = this.DB.問卷題目類型檔.Where(o => o.主索引 == (int)q.對應問卷題目類型索引).First().問卷類型名稱,
                    Required = q.是否必填 ? "on" : null,
                    Sort = q.排序,
                    IsCanDelete = CheckTopicIsCanDelete(ID, q.主索引)
                };
                var opList = this.DB.問卷選項檔.Where(o => o.對應問卷題目索引 == qTmep.ID).ToList();
                foreach (var op in opList)
                    qTmep.Option.Add(new Option() { OptionID = op.主索引, OptionValue = op.題目選項內容, IsCanDeleteOption = CheckTopicIsOptionCanDelete(ID, q.主索引, op.主索引) });
                questions.Add(qTmep);
            }
            questions = questions.OrderByDescending(o => o.Sort).ToList();
            result.Data.Topic = questions;
            PublicMethodRepository.HtmlDecode(result);
            foreach (var topci in result.Data.Topic)
            {
                PublicMethodRepository.HtmlDecode(topci);
                foreach (var op in topci.Option)
                    PublicMethodRepository.HtmlDecode(op);
            }
            return result;
        }

        public QuestionDetailsDataModel DoGetDetailsByUserWitnEdit(int ID)
        {
            var questMain = this.DB.問卷主檔.Where(o => o.主索引 == ID).FirstOrDefault();
            var ansData = this.DB.問卷答案主檔.Where(o => o.對應問卷主檔索引 == ID && o.填寫人ID == UserFrontProvider.Instance.User.ID).FirstOrDefault();
            if (questMain == null)
                throw new Exception("無法取得該問卷");
            string currentUrl = "http://" + HttpContext.Current.Request.Url.Authority;
            QuestionDetailsDataModel result = new QuestionDetailsDataModel()
            {
                ID = questMain.主索引,
                Title = questMain.問卷標題,
                Description = questMain.問卷描述,
                OpeningTime = questMain.開放時間起始日.ToString("yyyy\\/MM\\/dd"),
                EndTime = questMain.開放時間結束日.ToString("yyyy\\/MM\\/dd"),
                SendCount = questMain.預計發出量,
                Sort = questMain.排序,
                Status = questMain.是否上架 ? "Y" : "N",
                IsSignIn = questMain.是否需要登入 ? "on" : "off",
                Url = currentUrl,
                CreateDate = questMain.建立日期
            };

            var topicDetails = this.DB.問卷題目檔.Where(o => o.對應問卷索引 == ID).ToList();
            List<Topic> questions = new List<Topic>();
            foreach (var t in topicDetails)
            {
                Topic qTmep = new Topic()
                {
                    ID = t.主索引,
                    TopicContent = t.問卷題目內容,
                    TopicType = (int)t.對應問卷題目類型索引,
                    TopicTypeName = this.DB.問卷題目類型檔.Where(o => o.主索引 == (int)t.對應問卷題目類型索引).First().問卷類型名稱,
                    Required = t.是否必填 ? "on" : null,
                    Sort = t.排序,
                    IsCanDelete = CheckTopicIsCanDelete(ID, t.主索引),
                    Answer = DoGetQuestionAnswerByUser(ID, t.主索引)
                };
                var opList = this.DB.問卷選項檔.Where(o => o.對應問卷題目索引 == qTmep.ID).ToList();
                foreach (var op in opList)
                    qTmep.Option.Add(new Option() { OptionID = op.主索引, OptionValue = op.題目選項內容, IsCanDeleteOption = CheckTopicIsOptionCanDelete(ID, t.主索引, op.主索引) });
                questions.Add(qTmep);
            }
            questions = questions.OrderByDescending(o => o.Sort).ToList();
            result.Data.Topic = questions;
            PublicMethodRepository.HtmlDecode(result);
            foreach (var topic in result.Data.Topic)
            {
                PublicMethodRepository.HtmlDecode(topic);
                foreach (var op in topic.Option)
                    PublicMethodRepository.HtmlDecode(op);
            }
            return result;
        }

        /// <summary>
        /// 取得User回答 For User/QuestionApply
        /// </summary>
        /// <param name="questionID"></param>
        /// <param name="topicID"></param>
        /// <returns></returns>
        public List<QuestionDetailsAnswerModel> DoGetQuestionAnswerByUser(int questionID, int topicID)
        {
            List<QuestionDetailsAnswerModel> ansModels = new List<QuestionDetailsAnswerModel>();
            var getTopic = this.DB.問卷題目檔.Where(o => o.主索引 == topicID).FirstOrDefault();
            if (getTopic == null)
                throw new Exception("無法取得題目資料");

            var options = this.DB.問卷選項檔
                    .Where(o => o.對應問卷主檔索引 == questionID
                    && o.對應問卷題目索引 == topicID
                    ).ToList();

            var getAns = this.DB.問卷答案主檔.Where(o => o.對應問卷主檔索引 == questionID
            && o.填寫人ID == UserFrontProvider.Instance.User.ID).FirstOrDefault();
            if (getAns == null)
                throw new Exception("該用戶尚未填寫過問卷或填寫資料已被移除");

            var ans = this.DB.問卷答案明細檔
                    .Where(o => o.對應問卷答案索引 == getAns.主索引
                    && o.對應問卷題目索引 == topicID).ToList();

            int[] optionIdentify = new int[] { 1, 2, 3 };
            foreach (var a in ans)
            {
                QuestionDetailsAnswerModel temp = new QuestionDetailsAnswerModel()
                {
                    Index = a.主索引,
                    QuestionID = topicID,
                    QuestionTypeID = a.對應問題類型索引,
                    Required = getTopic.是否必填
                };
                if (optionIdentify.Contains((int)getTopic.對應問卷題目類型索引))
                    temp.Value = a.問卷答案選項值.ToString();
                else
                    temp.Value = a.問卷答案文字值;

                ansModels.Add(temp);
            }
            foreach (var d in ansModels)
                PublicMethodRepository.HtmlDecode(d);
            return ansModels;
        }

        /// <summary>
        /// 判斷題目是否有人填寫 (可否刪除)
        /// </summary>
        /// <param name="questionID"></param>
        /// <param name="topicID"></param>
        /// <returns></returns>
        private bool CheckTopicIsCanDelete(int questionID, int topicID)
        {
            var ansList = this.DB.問卷答案主檔.Where(o => o.對應問卷主檔索引 == questionID).Select(s => s.主索引).ToList();

            var hasAns = (this.DB.問卷答案明細檔.Where(o => ansList.Contains(o.對應問卷答案索引) && o.對應問卷題目索引 == topicID).Count() == 0);

            return hasAns;
        }

        /// <summary>
        /// 判斷題目是否有人填寫 (可否刪除)
        /// </summary>
        /// <param name="questionID"></param>
        /// <param name="topicID"></param>
        /// <returns></returns>
        private bool CheckTopicIsOptionCanDelete(int questionID, int topicID, int optionID)
        {
            var ansList = this.DB.問卷答案主檔.Where(o => o.對應問卷主檔索引 == questionID).Select(s => s.主索引).ToList();

            var hasAns = (this.DB.問卷答案明細檔.Where(o => ansList.Contains(o.對應問卷答案索引)
                 && o.對應問卷題目索引 == topicID && o.問卷答案選項值 == optionID).Count() == 0);

            return hasAns;
        }

        public QuestionListResultModel DoGetList(QuestionListFilterModel filterModel)
        {
            PublicMethodRepository.FilterXss(filterModel);
            QuestionListResultModel result = new QuestionListResultModel();
            List<QuestionListDataModel> data = new List<QuestionListDataModel>();
            try
            {
                data = DB.問卷主檔
                    .ToList()
                    .Select(o => new QuestionListDataModel()
                    {
                        ID = o.主索引,
                        Title = o.問卷標題,
                        BeginDateStr = o.開放時間起始日.ToString("yyyy\\/MM\\/dd"),
                        EndDateStr = o.開放時間結束日.ToString("yyyy\\/MM\\/dd"),
                        IsLogin = o.是否需要登入,
                        Status = o.是否上架,
                        Sort = o.排序,
                        CreateDate = o.建立日期
                    })
                    .ToList();

                //關鍵字搜尋
                if (!string.IsNullOrEmpty(filterModel.QueryString))
                {
                    this.ListFilter(filterModel.QueryString, ref data);
                }
                ////發佈日期搜尋
                //if (!string.IsNullOrEmpty(filterModel.PublishDate))
                //{
                //    this.NewsListDateFilter(Convert.ToDateTime(filterModel.PublishDate), ref data);
                //}

                ////前台顯示
                //if (!string.IsNullOrEmpty(filterModel.DisplayForFrontEnd))
                //{
                //    this.NewsListStatusFilter(filterModel.DisplayForFrontEnd, "F", ref data);
                //}

                ////首頁顯示
                //if (!string.IsNullOrEmpty(filterModel.DisplayForHomePage))
                //{
                //    this.NewsListStatusFilter(filterModel.DisplayForHomePage, "H", ref data);
                //}

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

        public int DoSaveData(QuestionDetailsDataModel saveModel)
        {
            問卷主檔 questMain;
            bool isInsert = true;
            if (saveModel.ID == 0)
            {
                questMain = new 問卷主檔();
                questMain.建立人員 = UserProvider.Instance.User.ID;
                questMain.建立日期 = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                isInsert = false;
                questMain = this.DB.問卷主檔.Where(s => s.主索引 == saveModel.ID).FirstOrDefault();
                questMain.問卷網址 = saveModel.Url;
            }

            DateTime endDate = Convert.ToDateTime(saveModel.EndTime);
            DateTime endDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

            questMain.問卷標題 = saveModel.Title;
            questMain.問卷描述 = saveModel.Description;
            questMain.開放時間起始日 = Convert.ToDateTime(saveModel.OpeningTime);
            questMain.開放時間結束日 = endDateTime;
            questMain.預計發出量 = saveModel.SendCount;
            questMain.排序 = saveModel.Sort;
            questMain.是否上架 = saveModel.Status == "Y" ? true : false;
            questMain.是否需要登入 = saveModel.IsSignIn == null ? false : true;
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

            if (isInsert)
            {
                var update = this.DB.問卷主檔.Where(o => o.主索引 == questMain.主索引).FirstOrDefault();
                update.問卷網址 = "http://" + HttpContext.Current.Request.Url.Authority + "/Question/Content?ID=" + questMain.主索引;
                this.DB.Entry(questMain).State = EntityState.Modified;
                try
                {
                    this.DB.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            int identityId = (int)questMain.主索引;
            SaveQuestionSetting(identityId, saveModel);
            return identityId;
        }

        private void SaveQuestionSetting(int identityId, QuestionDetailsDataModel model)
        {
            try
            {
                PublicMethodRepository.FilterXss(model);
                foreach (var topic in model.Data.Topic)
                {
                    PublicMethodRepository.FilterXss(topic);
                    foreach (var option in topic.Option)
                        PublicMethodRepository.FilterXss(option);
                }
                #region 刪除

                if (model.Data.Topic.Count == 0)
                {
                    var questionList = this.DB.問卷題目檔.Where(o => o.對應問卷索引 == identityId).ToList();
                    var optionList = this.DB.問卷選項檔.Where(o => o.對應問卷主檔索引 == identityId).ToList();
                    if (questionList.Count > 0)
                    {
                        this.DB.問卷題目檔.RemoveRange(questionList);
                        this.DB.SaveChanges();
                    }
                    if (optionList.Count > 0)
                    {
                        this.DB.問卷選項檔.RemoveRange(optionList);
                        this.DB.SaveChanges();
                    }
                    return;
                }
                List<int> beforeTopicIdList = new List<int>();
                List<int> beforeTopicOptionIdList = new List<int>();
                foreach (var q in model.Data.Topic)
                {
                    if (q.BeforeID != null)
                    {
                        beforeTopicIdList.Add((int)q.BeforeID);
                        foreach (var op in q.Option)
                        {
                            if (op.BeforeOptionID != null)
                                beforeTopicOptionIdList.Add((int)op.BeforeOptionID);
                        }
                    }
                }

                if (beforeTopicOptionIdList.Count == 0)
                {
                    this.DB.問卷題目檔.RemoveRange(this.DB.問卷題目檔.Where(o => o.對應問卷索引 == model.ID).ToList());
                    this.DB.SaveChanges();
                }
                else
                {
                    var delOptionList = this.DB.問卷選項檔.Where(o => !beforeTopicOptionIdList.Contains(o.主索引)
                    && o.對應問卷主檔索引 == identityId).ToList();
                    this.DB.問卷選項檔.RemoveRange(delOptionList);
                    this.DB.SaveChanges();
                }

                if (beforeTopicIdList.Count == 0)
                {
                    this.DB.問卷選項檔.RemoveRange(this.DB.問卷選項檔.Where(o => o.對應問卷主檔索引 == model.ID).ToList());
                    this.DB.SaveChanges();
                }
                else
                {
                    var delTopicList = this.DB.問卷題目檔.Where(o => !beforeTopicIdList.Contains(o.主索引)
                    && o.對應問卷索引 == identityId).ToList();
                    this.DB.問卷題目檔.RemoveRange(delTopicList);
                    this.DB.SaveChanges();
                }

                #endregion 刪除

                foreach (var q in model.Data.Topic)
                {
                    if (q.BeforeID != null)
                    {
                        var topic = this.DB.問卷題目檔.Where(o => o.主索引 == q.BeforeID && o.對應問卷索引 == identityId).FirstOrDefault();

                        #region 更新原有題目檔

                        topic.對應問卷索引 = identityId;
                        topic.問卷題目內容 = q.TopicContent;
                        topic.排序 = q.Sort;
                        topic.對應問卷題目類型索引 = q.TopicType;
                        topic.是否必填 = q.Required == null ? false : true;
                        topic.更新人員 = UserProvider.Instance.User.ID;
                        topic.更新日期 = DateTime.UtcNow.AddHours(8);

                        #endregion 更新原有題目檔

                        this.DB.Entry(topic).State = EntityState.Modified;
                        this.DB.SaveChanges();

                        foreach (var op in q.Option)
                        {
                            問卷選項檔 optionSetting = null;

                            if (op.BeforeOptionID != null)
                            {
                                optionSetting = this.DB.問卷選項檔.Where(o => o.主索引 == op.BeforeOptionID && o.對應問卷題目索引 == q.BeforeID && o.對應問卷主檔索引 == identityId).FirstOrDefault();

                                #region 更新原有項目檔

                                optionSetting.題目選項內容 = op.OptionValue;
                                optionSetting.對應問卷主檔索引 = identityId;
                                optionSetting.對應問卷題目索引 = (int)q.BeforeID;
                                optionSetting.對應問題類型索引 = q.TopicType;
                                optionSetting.排序 = op.Sort;

                                #endregion 更新原有項目檔

                                this.DB.Entry(optionSetting).State = EntityState.Modified;
                                this.DB.SaveChanges();
                            }
                            else
                            {
                                #region 新增項目檔

                                optionSetting = new 問卷選項檔();
                                optionSetting.題目選項內容 = op.OptionValue;
                                optionSetting.對應問卷主檔索引 = identityId;
                                optionSetting.對應問卷題目索引 = (int)q.BeforeID;
                                optionSetting.對應問題類型索引 = q.TopicType;
                                optionSetting.排序 = op.Sort;

                                #endregion 新增項目檔

                                this.DB.問卷選項檔.Add(optionSetting);
                                this.DB.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        #region 新增

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
                                對應問題類型索引 = q.TopicType,
                                排序 = op.Sort
                            };
                            this.DB.問卷選項檔.Add(optionSetting);
                            this.DB.SaveChanges();
                        }

                        #endregion 新增
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void NewsListFilterLanguage(Language language, ref List<NewsListDataModel> data)
        //{
        //    var r = data.Where(s => s.Language == language.GetCode()).ToList();
        //    data = r;
        //}

        /// <summary>
        /// 列表關鍵字搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        private void ListFilter(string filterStr, ref List<QuestionListDataModel> data)
        {
            var r = data.Where(s => s.Title.Contains(filterStr)).ToList();
            data = r;
        }

        /// <summary>
        /// 日期條件搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        //private void NewsListDateFilter(DateTime publishdate, ref List<QuestionListDataModel> data)
        //{
        //    var r = data.Where(s => s.發稿時間 == publishdate).ToList();
        //    data = r;
        //}

        /// <summary>
        /// 是否上架搜尋
        /// </summary>
        /// <param name="filterStr"></param>
        /// <param name="data"></param>
        //private void ListStatusFilter(string status, string displayMode, ref List<QuestionListDataModel> data)
        //{
        //    List<新聞> result = null;
        //    if (displayMode == "F")
        //    {
        //        if (status == "Y")
        //            result = data.Where(s => s.前台顯示 == true).ToList();
        //        else
        //            result = data.Where(s => s.前台顯示 == false).ToList();
        //    }
        //    else if (displayMode == "H")
        //        if (status == "Y")
        //            result = data.Where(s => s.首頁顯示 == true).ToList();
        //        else
        //            result = data.Where(s => s.首頁顯示 == false).ToList();
        //    data = result;
        //}

        /// <summary>
        /// 取出分頁資料
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="data"></param>
        private void ListPageList(int currentPage, ref List<QuestionListDataModel> data, out PaginationResult pagination)
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
        private void ListSort(string sortCloumn, string status, ref List<QuestionListDataModel> data)
        {
            switch (sortCloumn)
            {
                case "sortBegingDate/asc":
                    data = data.OrderBy(o => o.BeginDateStr).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortBegingDate/desc":
                    data = data.OrderByDescending(o => o.BeginDateStr).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortEndDate/asc":
                    data = data.OrderBy(o => o.EndDateStr).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortEndDate/desc":
                    data = data.OrderByDescending(o => o.EndDateStr).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortStatus/asc":
                    data = data.OrderBy(o => o.Status).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortStatus/desc":
                    data = data.OrderByDescending(o => o.Status).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortIsLogin/asc":
                    data = data.OrderBy(o => o.IsLogin).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortIsLogin/desc":
                    data = data.OrderByDescending(o => o.IsLogin).ThenByDescending(g => g.Sort).ToList();
                    break;

                case "sortIndex/asc":
                    data = data.OrderBy(o => o.Sort).ThenByDescending(o => o.CreateDate).ToList();
                    break;

                case "sortIndex/desc":
                    data = data.OrderByDescending(o => o.Sort).ThenByDescending(o => o.CreateDate).ToList();
                    break;

                default:
                    data = data.OrderByDescending(o => o.Sort).ThenByDescending(o => o.CreateDate).ToList();
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