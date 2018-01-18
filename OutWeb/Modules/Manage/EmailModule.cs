using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using OutWeb.Controllers;
using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Models.Manage.ManageNotificationModels;
using OutWeb.Models.Manage.SerializationModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using OutWeb.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 最新消息列表模組
    /// </summary>
    public class EmailModule : ListModuleService
    {
        private DBEnergy m_DB = new DBEnergy();
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public override void DoDeleteByID(int ID)
        {
            var data = this.DB.EMAIL.Where(s => s.EMAIL_ID == ID).FirstOrDefault();
            if (data == null)
            {
                logger.Info("[刪除Email] 查無此Email，可能已被移除"); //將錯誤記在NLog裡
                throw new Exception("查無此Email，可能已被移除");
            }

            if (data.STATUS != 0) //非草稿不得刪除
            {
                logger.Info("[刪除Email]刪除Email錯誤，因為此Email為非草稿狀態，ID:" + ID); //將錯誤記在NLog裡
                throw new Exception("非'未啟動'狀態則無法進行刪除");
            }

            var findExitTIL = this.DB.檔案.Where(x => x.對應索引 == ID && x.對應名稱 == "Email" && x.檔案模式 == "M" && x.檔案格式 == "F").FirstOrDefault();

            //刪除上傳的檔案
            if (findExitTIL != null)
            {
                this.DB.檔案.Remove(findExitTIL);
                File.Delete(string.Concat(rootPath, findExitTIL.檔案路徑));
            }

            try
            {
                this.DB.EMAIL.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Info("[刪除Email]刪除錯誤，錯誤訊息:" + ex.Message); //將錯誤記在NLog裡
                throw new Exception("Email刪除發生錯誤");
            }
        }

        public override object DoGetDetailsByID(int ID)
        {
            EmailDataModel result = new EmailDataModel();
            EMAIL data = DB.EMAIL.Where(w => w.EMAIL_ID == ID).FirstOrDefault();
            if (data != null)
            {
                PublicMethodRepository.HtmlDecode(data);
                result.Data = data;
                return result;
            }
            else
            {
                return null;
            }
        }

        public override object DoGetList<TFilter>(TFilter model, Language language)
        {
            return null;
        }

        public override int DoSaveData(FormCollection form, Language language, int? ID = null, List<HttpPostedFileBase> filesMember = null, List<HttpPostedFileBase> filesAttachment = null)
        {
            EMAIL saveModel;
            FileRepository fileRepository = new FileRepository();

            if (ID == 0)
            {
                saveModel = new EMAIL();
                saveModel.INSERT_NAME = UserProvider.Instance.User.ID;
                saveModel.INSERT_DATE = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.EMAIL.Where(s => s.EMAIL_ID == ID).FirstOrDefault();
            }
            saveModel.UPDATE_NAME = UserProvider.Instance.User.ID;
            saveModel.UPDATE_DATE = DateTime.UtcNow.AddHours(8);
            saveModel.TITLE = form["TITLE"];
            saveModel.PUBLISHER = form["PUBLISHER"];
            saveModel.DELIEVER_TYPE = form["DELIEVER_TYPECheck"];
            saveModel.MAIL_FORM = form["MAIL_FORM"];
            saveModel.MAIL_SUBJECT = form["MAIL_SUBJECT"];
            saveModel.MAIL_BODY = form["MAIL_BODY"];
            saveModel.DELIEVER_DATE = DateTime.UtcNow.AddHours(8);

            //1.草稿 2.立即送出 3.預約送出 4.預約取消
            string formType = form["type"];
            if (formType == "save")
                saveModel.STATUS = 0;
            else
            {
                saveModel.STATUS = 1;
                saveModel.SEND_DATE = DateTime.UtcNow.AddHours(8);
                saveModel.SEND_NAME = UserProvider.Instance.User.ID;
            }

            PublicMethodRepository.FilterXss(saveModel);

            if (ID > 0)
                this.DB.Entry(saveModel).State = EntityState.Modified;
            else
                this.DB.EMAIL.Add(saveModel);

            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            int identityId = (int)saveModel.EMAIL_ID;

            #region 檔案處理 聯絡人清單

            FileSaveHandler(form, identityId, "MemberFile", "Email", filesMember, FileUploadIdentifyType.MODE1);

            #endregion 檔案處理 聯絡人清單

            #region 檔案處理 附件檔

            FileSaveHandler(form, identityId, "AttachmentFile", "Email", filesAttachment, FileUploadIdentifyType.MODE2);

            #endregion 檔案處理 附件檔

            //建立json
            if (formType == "send")
                CreateJsonFileFromMailInfo(identityId);
            return identityId;
        }

        /// <summary>
        /// 檔案儲存Handler
        /// </summary>
        /// <param name="form">表單資料</param>
        /// <param name="identityId">對應的表單主索引</param>
        /// <param name="oldFileIdentifyString">舊檔識別字串</param>
        /// <param name="actionName">對應的功能名稱</param>
        /// <param name="files">檔案集合</param>
        /// <param name="uploadType">檔案類別</param>
        private void FileSaveHandler(FormCollection form,
            int identityId,
            string oldFileIdentifyString,
            string actionName,
            List<HttpPostedFileBase> files,
            FileUploadIdentifyType uploadType = FileUploadIdentifyType.NOTSET)
        {
            FileRepository fileRepository = new FileRepository();

            try
            {
                #region 檔案處理

                List<int> oldFileFileList = new List<int>();

                #region 將原存在的Server檔案保留 記錄檔案ID

                //將原存在的Server檔案保留 記錄檔案ID
                foreach (var f in form.Keys)
                {
                    if (f.ToString().StartsWith(oldFileIdentifyString))
                    {
                        var id = Convert.ToInt16(form[f.ToString().Split('.')[0] + ".ID"]);
                        if (!oldFileFileList.Contains(id))
                            oldFileFileList.Add(id);
                    }
                }

                #endregion 將原存在的Server檔案保留 記錄檔案ID

                #region 建立檔案模型

                FilesModel filesModel = new FilesModel()
                {
                    ActionName = actionName,
                    ID = identityId,
                    OldFileIds = oldFileFileList,
                    UploadIdentify = uploadType
                };

                #endregion 建立檔案模型

                #region 若有null則是前端html的name重複於ajax formData名稱

                if (files != null)
                {
                    if (files.Count > 0)
                        files.RemoveAll(item => item == null);
                }

                #endregion 若有null則是前端html的name重複於ajax formData名稱

                #region 存檔

                fileRepository.UploadFile("Post", filesModel, files, "M");
                fileRepository.SaveFileToDB(filesModel);

                #endregion 存檔

                #endregion 檔案處理
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Email Output

        private void CreateJsonFileFromMailInfo(int ID, bool send = false)
        {
            try
            {
                List<MailStruc> msList = ReadExcelContent(ID);
                foreach (var ms in msList)
                    saveJson(ms);
                //if (send)
                //    TestSendMail(msList);
            }
            catch (Exception ex)
            {
                throw new Exception("建立Json檔發生錯誤", ex);
            }
        }

        ///// <summary>
        ///// 測試發信
        ///// </summary>
        //private void TestSendMail(List<MailStruc> mailObjData)
        //{
        //    #region 測試發信

        //    MailerAPI.MailSetting setting = new MailerAPI.MailSetting()
        //    {
        //        SmtpServer = "smtp.gmail.com",
        //        Port = 587,
        //        UserName = "***信箱帳號***",
        //        Password = "***信箱密碼***",
        //        EnableSsl = true
        //    };
        //    foreach (var mailstruc in mailObjData)
        //    {
        //        MailerAPI.MailInfo info = new MailerAPI.MailInfo();
        //        info.Subject = mailstruc.mailinfo.subject;
        //        info.Body = new System.Text.StringBuilder(mailstruc.mailinfo.body);
        //        info.To.AddRange(mailstruc.mailinfo.to);

        //        MailerAPI.Mailer mailer = new MailerAPI.Mailer(setting, info);
        //        mailer.SendMail();
        //    }

        //    #endregion 測試發信
        //}

        //讀取Excel檔案
        private List<MailStruc> ReadExcelContent(int ID)
        {
            EmailDataModel details = (this.DoGetDetailsByID(ID) as EmailDataModel);
            EMAIL model = details.Data;
            //取檔案
            FileModule fileModule = new FileModule();

            //聯絡人
            var contactListFile = this.DB.檔案
                .Where(o => o.對應索引 == ID
                && o.對應名稱.StartsWith("EMAIL")
                && o.檔案模式 == "M"
                && o.檔案格式 == "F"
                && o.識別碼 == (int)FileUploadIdentifyType.MODE1)
                .FirstOrDefault();

            if (contactListFile == null)
                throw new Exception("[Email維護啟動發送]於DB中找不到ID相對應的Excel檔案");

            //附件檔
            var attachmentFiles = this.DB.檔案
                .Where(o => o.對應索引 == ID
                && o.對應名稱.StartsWith("EMAIL")
                && o.檔案模式 == "M"
                && o.檔案格式 == "F"
                && o.識別碼 == (int)FileUploadIdentifyType.MODE2)
                .ToList();
            List<string> attachmentPathList = new List<string>();
            foreach (var file in attachmentFiles)
                attachmentPathList.Add(string.Concat(rootPath, file.檔案路徑));

            string mailFrom = model.MAIL_FORM;



            // open xlsx
            XLWorkbook excelWB = new XLWorkbook(string.Concat(rootPath, contactListFile.檔案路徑));
            IXLWorksheet sheet = excelWB.Worksheet(1);
            int RowCount = sheet.RowsUsed().Count();
            List<MailStruc> msList = new List<MailStruc>();
            try
            {
                for (int i = 2; i <= RowCount; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 1).Value)))
                    {
                        MailInfo info = new MailInfo()
                        {
                            subject = model.MAIL_SUBJECT,
                            from = mailFrom,
                            filepath = attachmentPathList.Count > 0 ? attachmentPathList.ToArray() : new string[] { },
                            body = RenderViewToString("ViewTemplate", "Email", model)
                        };

                        info.to = new string[] { sheet.Cell(i, 1).Value.ToString() };

                        msList.Add(new MailStruc
                        {
                            mailinfo = info,
                            createtime = DateTime.UtcNow.AddHours(8),
                            sendresult = new List<SendResult>()
                        });
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("[簡訊維護啟動發送] 讀取Excel時錯誤，錯誤如下:" + ex);
            }
            return msList;
        }

        /// <summary>
        /// 將MailStruc序列化JSON字串並儲存成JSON檔至指定資料夾
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        private string saveJson(MailStruc ms)
        {
            string json = JsonConvert.SerializeObject(ms);
            string filename = ms.id + ".json";
            string path = HttpContext.Current.Server.MapPath("~/MailJson/" + filename);
            try
            {
                File.WriteAllText(path, json, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception("產生JSON信件錯誤", ex);
            }
            return ms.id;
        }

        /// <summary>
        /// 生成View為Html 字串
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="viewName"></param>
        /// <param name="viewData"></param>
        /// <returns></returns>
        private static string RenderViewToString(string controllerName, string viewName, object model)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                //var emailControllerContext = new ControllerContext(new HttpContextWrapper(new HttpContext(new HttpRequest(null, "http://google.com", null), new HttpResponse(null))), routeData, new ViewTemplateController());
                var emailControllerContext = new ControllerContext(new HttpContextWrapper(HttpContext.Current), routeData, new ViewTemplateController());

                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(emailControllerContext, viewName, "", false);

                var viewContext = new ViewContext(emailControllerContext, razorViewResult.View, new ViewDataDictionary(model), new TempDataDictionary(), writer);
                razorViewResult.View.Render(viewContext, writer);
                return writer.ToString();
            }
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

        #endregion Email Output
    }
}