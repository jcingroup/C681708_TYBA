using OutWeb.Entities;
using OutWeb.Models;
using OutWeb.Models.FrontEnd.QuestionnairesModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutWeb.Modules.FontEnd
{
    public class QuestionnairesFrontModule : IDisposable
    {
        private DBEnergy DB = new DBEnergy();

        #region 分頁

        /// <summary>
        /// [前台] 列表分頁處理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public QuestListViewModel ListPagination(QuestListViewModel model, int page, int pageSize)
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

        #endregion 分頁

        public QuestListViewModel GetList(int? page = 1)
        {
            QuestListViewModel result = new QuestListViewModel();
            int pageSize = Convert.ToInt16(PublicMethodRepository.GetConfigAppSetting("DefaultPageSize"));
            DateTime today = DateTime.UtcNow.AddHours(8);
            var data = this.DB.問卷主檔.ToList()
                .Where(o => o.是否上架)
                .Select(o => new QuestionListData()
                {
                    ID = o.主索引,
                    CreateDate = o.建立日期,
                    IsLogin = o.是否需要登入,
                    Status = o.是否上架,
                    Title = o.問卷標題,
                    Sort = o.排序,
                    PeopleNumber = DB.問卷答案主檔.Where(s => s.對應問卷主檔索引 == o.主索引).Count(),
                    IsFinish = (o.開放時間起始日 < today && o.開放時間結束日 < today),
                    IsNotyet = (o.開放時間起始日 > today && o.開放時間結束日 > today)
                })
                .ToList();
            foreach (var d in data)
                PublicMethodRepository.HtmlDecode(d);
            data = data.OrderByDescending(o => o.Sort).ThenByDescending(o => o.CreateDate).ToList();
            result.Data = data;
            result = ListPagination(result, (int)page, pageSize);
            return result;
        }

        public QuestListViewModel GetListByUser(int? page = 1)
        {
            QuestListViewModel result = new QuestListViewModel();
            int pageSize = Convert.ToInt16(PublicMethodRepository.GetConfigAppSetting("DefaultPageSize"));
            DateTime today = DateTime.UtcNow.AddHours(8);

            var data =
                  this.DB.問卷主檔.Join(this.DB.問卷答案主檔,
                    t1 => t1.主索引,
                    t2 => t2.對應問卷主檔索引,
                    (main, ans) => new { Main = main, Ans = ans })
                    .ToList()
                    .Where(o => o.Ans.填寫人ID == UserFrontProvider.Instance.User.ID)
                   .Select(o => new QuestionListData()
                   {
                       ID = o.Main.主索引,
                       CreateDate = o.Main.建立日期,
                       IsLogin = o.Main.是否需要登入,
                       Status = o.Main.是否上架,
                       Title = o.Main.問卷標題,
                       Sort = o.Main.排序,
                       PeopleNumber = DB.問卷答案主檔.Where(s => s.對應問卷主檔索引 == o.Main.主索引).Count(),
                       IsFinish = !(o.Main.開放時間起始日 <= today && o.Main.開放時間結束日 >= today),
                       IsNotyet = (o.Main.開放時間起始日 > today && o.Main.開放時間結束日 >= today)
                   })
                   .ToList();
            foreach (var d in data)
                PublicMethodRepository.HtmlDecode(d);
            data = data.OrderByDescending(o => o.Sort).ThenByDescending(o => o.CreateDate).ToList();
            result.Data = data;
            result = ListPagination(result, (int)page, pageSize);
            return result;
        }

        /// <summary>
        /// 驗證必填題目
        /// </summary>
        /// <param name="group"></param>
        private void ValidRequiredQuestion(IEnumerable<IGrouping<int, QuestionDetailsAnswerModel>> query)
        {
            foreach (IGrouping<int, QuestionDetailsAnswerModel> group in query)
            {
                foreach (var question in group)
                {
                    if (question.QuestionTypeID == 1 || question.QuestionTypeID == 2)
                    {
                        bool chk = false;
                        group.All(o =>
                        {
                            if (o.Required && !string.IsNullOrEmpty(o.Value))
                            {
                                chk = true;
                                return false;
                            }
                            return true;
                        });
                        if (question.Required && !chk)
                            throw new Exception(string.Format("第{0}題為必填", question.Index));
                    }
                    else
                    {
                        if (question.Required && string.IsNullOrEmpty(question.Value) && group.Count() == 1)
                        {
                            throw new Exception(string.Format("第{0}題為必填", question.Index));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 刪除User在該問卷下的回答
        /// </summary>
        /// <param name="ID"></param>
        public void DeleteAllAnswerByUser(int ID)
        {
            var ans = this.DB.問卷答案主檔.Where(o => o.對應問卷主檔索引 == ID
              && o.填寫人ID == UserFrontProvider.Instance.User.ID).FirstOrDefault();
            if (ans == null)
                throw new Exception("找不到該問卷回答檔，可能已被刪除");
            this.DB.問卷答案明細檔.RemoveRange(this.DB.問卷答案明細檔.Where(o => o.對應問卷答案索引 == ans.主索引));
            this.DB.問卷答案主檔.Remove(ans);
            this.DB.SaveChanges();
        }

        /// <summary>
        /// 新增問卷答案
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Answer"></param>
        public void InsertAnswer(int ID, List<QuestionDetailsAnswerModel> Answer)
        {
            if (UserFrontProvider.Instance.User != null)
            {
                var chkHasAns = this.DB.問卷答案主檔.Where(o => o.對應問卷主檔索引 == ID && o.填寫人ID == UserFrontProvider.Instance.User.ID).ToList();
                if (chkHasAns.Count > 0)
                    throw new Exception("您已經填寫過此問卷，請勿重複填寫");
            }
            //依題目groupby
            IEnumerable<IGrouping<int, QuestionDetailsAnswerModel>> query =
                            Answer.GroupBy(x => x.QuestionID);

            #region 驗證必填

            ValidRequiredQuestion(query);

            #endregion 驗證必填

            List<問卷答案明細檔> ansDetails = new List<問卷答案明細檔>();
            foreach (IGrouping<int, QuestionDetailsAnswerModel> question in query)
            {
                foreach (var ansOption in question)
                {
                    問卷答案明細檔 ansDetail = new 問卷答案明細檔()
                    {
                        回答日期 = DateTime.UtcNow.AddHours(8),
                        對應問卷答案索引 = 0,
                        對應問題類型索引 = ansOption.QuestionTypeID,
                        對應問卷題目索引 = ansOption.QuestionID,
                    };

                    int[] optionIdentify = new int[] { 1, 2, 3 };

                    if (optionIdentify.Contains(ansOption.QuestionTypeID))
                    {
                        if (!string.IsNullOrEmpty(ansOption.Value))
                            ansDetail.問卷答案選項值 = Convert.ToInt16(ansOption.Value);
                        else
                            continue;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ansOption.Value))
                            ansDetail.問卷答案文字值 = ansOption.Value;
                        else
                            continue;
                    }

                    ansDetails.Add(ansDetail);
                }
            }

            if (ansDetails.Count > 0)
            {
                #region 回答主檔

                問卷答案主檔 ans = new 問卷答案主檔()
                {
                    對應問卷主檔索引 = ID,
                    填寫日期 = DateTime.UtcNow.AddHours(8),
                    填寫人 = UserFrontProvider.Instance.User == null ? null : UserFrontProvider.Instance.User.UserName,
                    填寫人ID = UserFrontProvider.Instance.User == null ? default(int?) : UserFrontProvider.Instance.User.ID,
                };
                DB.問卷答案主檔.Add(ans);
                DB.SaveChanges();

                #endregion 回答主檔

                ansDetails.All(o =>
                {
                    o.對應問卷答案索引 = ans.主索引;
                    return true;
                });
                foreach (var a in ansDetails)
                    PublicMethodRepository.FilterXss(a);
                DB.問卷答案明細檔.AddRange(ansDetails);
                DB.SaveChanges();
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