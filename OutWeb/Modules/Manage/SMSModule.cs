using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Models.Manage.ManageNotificationModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using OutWeb.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MitakeSMS;
using System.Configuration;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;
using NLog;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 簡訊模組
    /// </summary>
    public class SMSModule : ListModuleService
    {
        private DBEnergy m_DB = new DBEnergy();
        private MitakeSMSClass ms;
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        private List<SMSDetail> addList = null;
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public override void DoDeleteByID(int ID)
        {
            var data = this.DB.SMS.Where(s => s.SMS_ID == ID).FirstOrDefault();
            if (data == null)
            {
                logger.Info("[刪除簡訊] 查無此簡訊，可能已被移除"); //將錯誤記在NLog裡
                throw new Exception("查無此簡訊，可能已被移除");
            }

            if (data.STATUS != 0) //非草稿不得刪除
            {
                logger.Info("[刪除簡訊]刪除簡訊錯誤，因為此傳真為非草稿狀態，ID:" + ID); //將錯誤記在NLog裡
                throw new Exception("非'未啟動'狀態則無法進行刪除");
            }

            var findExitTIL = this.DB.檔案.Where(x => x.對應索引 == ID && x.對應名稱 == "SMS" && x.檔案模式 == "M" && x.檔案格式 == "F").FirstOrDefault();

            //刪除上傳的檔案
            if (findExitTIL != null)
            {
                this.DB.檔案.Remove(findExitTIL);
                File.Delete(string.Concat(rootPath, findExitTIL.檔案路徑));
            }

            try
            {
                this.DB.SMS.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Info("[刪除簡訊]刪除錯誤，錯誤訊息:" + ex.Message); //將錯誤記在NLog裡
                throw new Exception("簡訊刪除發生錯誤");
            }
        }

        public override object DoGetDetailsByID(int ID)
        {
            SMSDataModel result = new SMSDataModel();
            SMS data = DB.SMS.Where(w => w.SMS_ID == ID).FirstOrDefault();
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

        public override int DoSaveData(FormCollection form, Language language, int? ID = null, List<HttpPostedFileBase> images = null, List<HttpPostedFileBase> files = null)
        {
            return 0;
        }


        public ReturMsgParam DoSaveAndExecData(FormCollection form, Language language, int? ID = null, List<HttpPostedFileBase> images = null, List<HttpPostedFileBase> files = null)
        {
            SMS saveModel;
            ImageRepository imgepository = new ImageRepository();
            FileRepository fileRepository = new FileRepository();
            ReturMsgParam rm = new ReturMsgParam();

            if (!ID.HasValue || ID == 0)
            {
                saveModel = new SMS();
                saveModel.INSERT_NAME = UserProvider.Instance.User.ID;
                saveModel.INSERT_DATE = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.SMS.Where(s => s.SMS_ID == ID).FirstOrDefault();
                if (saveModel == null)
                {
                    rm.success = false;
                    rm.identityId = (int)ID;
                    rm.errorMsg = "[簡訊啟動發送】SMS Table找不到ID相對應的資料，ID為:" + (int)ID;
                    return rm;
                }

                //狀態為:立即送出、取消預約，則不能儲存檔案
                if (saveModel.STATUS == 1 || saveModel.STATUS == 3)
                {
                    rm.success = false;
                    rm.identityId = (int)ID;
                    rm.errorMsg = "[簡訊啟動發送】狀態為:立即送出、取消預約，則不能儲存檔案，ID為:" + (int)ID;
                    return rm;
                }
                saveModel.UPDATE_NAME = UserProvider.Instance.User.ID;
                saveModel.UPDATE_DATE = DateTime.UtcNow.AddHours(8);

            }

            saveModel.TITLE = form["TITLE"];
            saveModel.PUBLISHER = form["PUBLISHER"];
            saveModel.DELIEVER_TYPE = form["DELIEVER_TYPECheck"];

            if (form["DELIEVER_TYPECheck"] == "R")
            {
                string date = form["DELIEVER_DATE_DATE"] + " " + form["DELIEVER_DATE_TIME"] + ":00";
                DateTime dt = Convert.ToDateTime(date);
                saveModel.DELIEVER_DATE = dt;
            }
            else
            {
                saveModel.DELIEVER_DATE = DateTime.UtcNow.AddHours(8);
            }
            saveModel.STATUS = 0; //儲存草稿
            saveModel.CONTENTTEXT = form["CONTENTTEXT"];
            PublicMethodRepository.FilterXss(saveModel);
            if (ID.HasValue && ID != 0)
            {
                this.DB.Entry(saveModel).State = EntityState.Modified;
            }
            else
            {
                this.DB.SMS.Add(saveModel);
            }

            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }


            int identityId = (int)saveModel.SMS_ID;

            #region 檔案處理 

            FileSaveHandler(form, identityId, "FileData", "SMS", files);

            #endregion 檔案處理 



            #region 寄送三竹簡訊
            if (form["type"] == "send" || form["type"] == "test")
            {
                JsonParam jp = new JsonParam();

                if (form["type"] == "send")
                {
                    jp = ReadExcelContent(identityId, form["type"], null, null);
                }
                else
                {
                    jp = ReadExcelContent(identityId, form["type"], form["SENDTESTTEL"], form["SENDTESTNAME"]);
                }


                if (jp.success == true)
                {
                    jp = SendToMitake(identityId, jp.list, form["type"]);

                    if (jp.success == true)
                    {
                        rm.success = true;
                    }
                    else
                    {
                        rm.success = false;
                        logger.Info(jp.errorMsg); //將錯誤記在NLog裡
                    }
                }
                else
                {
                    rm.success = false;
                    logger.Info(jp.errorMsg); //將錯誤記在NLog裡
                }

            }

            #endregion


            //將是否執行成功的狀態回寫到SMSDetail
            if (addList != null)
            {
                foreach (var data in addList)
                {
                    var dd = this.DB.SMSDetail.Where(x => x.SMS_NO == data.SMS_NO).FirstOrDefault();
                    if (dd != null)
                    {
                        dd.IS_SUCCESS = rm.success;
                    }
                    this.DB.SaveChanges();
                }
                rm.count = addList.Count();
            }

            rm.identityId = identityId;
            return rm;

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
            FileUploadType uploadType = FileUploadType.NOTSET)
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



        #region 寄送給三竹
        /// <summary>
        /// 寄送給三竹簡訊
        /// </summary>
        /// <param name="ID">ID</param>
        /// <param name="list">IList<SMSDetail></param>
        /// <param name="type">傳送類型(test、send)></param>
        private JsonParam SendToMitake(int ID, IList<SMSDetail> list,string type)
        {
            JsonParam jp = new JsonParam();
            string response = ConfigurationManager.AppSettings["SMSResponse"];
            string encoding = ConfigurationManager.AppSettings["SMSEncoding"];

            var data = this.DB.SMS.Where(s => s.SMS_ID == ID).FirstOrDefault();

            if (data == null)
            {
                jp.errorMsg = "[簡訊維護啟動發送] 於資料庫中找不到ID相對應的SMS資料";
                jp.success = false;
                return jp;
            }

            MitakeSMSClass ms = GetSMSClassEntity();

            MitakeSMSClass.SendModel sm = new MitakeSMSClass.SendModel();
            List<MitakeSMSClass.SendMemberModel> smm = new List<MitakeSMSClass.SendMemberModel>();

            foreach (var lst in list)
            {
                smm.Add(new MitakeSMSClass.SendMemberModel { id = lst.SMS_NO, destName = lst.NAME, dstaddr = lst.TEL });

            }

            if (data.DELIEVER_TYPE == "R" && type!="test")
            {

                sm.dlvtime = (DateTime)data.DELIEVER_DATE;
            }


            sm.encoding = encoding;
            sm.member = smm;
            sm.response = response;
            sm.smbody = data.CONTENTTEXT;

            Task<MitakeSMSClass.SendReturnModel> srm;

            //將狀態改為傳入三竹中(預設為成功)
            if (data.DELIEVER_TYPE == "R" && type == "send")
            {
                data.STATUSDETAIL = 1;
            }
            else
            {
                data.STATUSDETAIL = 5;
            }

            srm = ms.SendSMS(sm);


            if (data.DELIEVER_TYPE == "I" && type == "send")
            {
                //立即啟動紀錄送出時間與Name
                data.SEND_NAME = UserProvider.Instance.User.ID;
                data.SEND_DATE = DateTime.UtcNow.AddHours(8);
                this.DB.SaveChanges();
            }


            if (srm != null && srm.Result.state == 0)
            {
                //將狀態改為三竹回傳成功
                if (data.DELIEVER_TYPE == "R" )
                {
                    if (type == "send")
                    {
                        //為預約啟動
                        data.STATUS = 2;
                        data.STATUSDETAIL = 3;
                    }

                }
                else
                {
                    if (type == "send")
                    {
                        //為立即啟動
                        data.STATUS = 1;
                        data.STATUSDETAIL = 7;
                    }

                }


                this.DB.SaveChanges();

                jp = SaveReplyToDB(ID, srm);

                if(jp.success == false)
                {
                    return jp;
                }
            }
            else
            {
                if (srm.Result.state == -2 || srm.Result.state == -3 || srm.Result.state == -4)
                {
                    if (type == "send")
                    {
                        //將狀態改為傳入三竹失敗
                        if (data.DELIEVER_TYPE == "R")
                        {
                            data.STATUSDETAIL = 6;
                        }
                        else
                        {
                            data.STATUSDETAIL = 2;
                        }
                    }

                }
                if (srm.Result.state == -5)
                {
                    if (type == "send")
                    {
                        //將狀態改為傳入三竹回傳失敗
                        if (data.DELIEVER_TYPE == "R")
                        {
                            data.STATUSDETAIL = 8;
                        }
                        else
                        {
                            data.STATUSDETAIL = 4;
                        }
                    }
                }

                this.DB.SaveChanges();

                jp.success = false;
                jp.errorMsg = "[簡訊維護啟動發送]呼叫MitakeSMS.dll SendSMS()，dll內部回傳錯誤訊息，錯誤原因: " + srm.Result.errorMsg;
                return jp;
            }


            jp.success = true;
            return jp;
        }
        #endregion

        #region MitakeSMS Class實例化
        /// <summary>
        /// MitakeSMS.dll Class實例化
        /// </summary>
        private MitakeSMSClass GetSMSClassEntity()
        {
            string user = ConfigurationManager.AppSettings["SMSUser"];
            string password = ConfigurationManager.AppSettings["SMSPassword"];
            string connectString = ConfigurationManager.AppSettings["SMSconnectString"];


            if( user == "" || password == "" || connectString == "")
            {
                throw new Exception("[三竹相關訊息設定]帳號、密碼、或連線字串有空值");
            }

            if (ms == null)
            {
                ms = new MitakeSMSClass(user,password, connectString);
            }

            return ms;
        }
        #endregion

        #region 儲存三竹及時回傳的訊息
        /// <summary>
        /// 儲存三竹及時回傳的訊息
        /// </summary>
        /// <param name="ID">ID</param>
        /// <param name="srm">Task<MitakeSMSClass.SendReturnModel>></param>
        private JsonParam SaveReplyToDB(int ID, Task<MitakeSMSClass.SendReturnModel> srm)
        {
            JsonParam jp = new JsonParam();
            SMSDetail dt = new SMSDetail();
            try
            {
                foreach (var data in srm.Result.data)
                {
                    var d = this.DB.SMSDetail.Where(x => x.SMS_ID == ID && x.SMS_NO == data.id).FirstOrDefault();
                    if (d != null)
                    {
                        d.MSG_ID = data.msgId;
                        d.STATUSCODE = data.status;
                    }

                    jp.success = true;
                    this.DB.SaveChanges();
                }
            }
            catch (Exception e)
            {
                jp.success = false;
                jp.errorMsg = "[簡訊維護啟動發送]將三竹回傳訊息存入DB時發生錯誤，錯誤如下:" + e.Message;
                return jp;
            }

            return jp;

        }
        #endregion

        #region 讀取Excel檔案和儲存人員資料
        /// <summary>
        /// 讀取Excel檔案和儲存人員資料至DB
        /// </summary>
        /// <param name="ID">ID</param>
        /// <param name="type">傳送類型(test、send)></param>
        /// <param name="testTel">測試人員的手機號碼></param>
        /// <param name="testName">測試人員的姓名></param>
        private JsonParam ReadExcelContent(int ID,string type,string testTel,string testName)
        {
            //正式送出
            if (type == "send")
            {
                //取檔案
                FileModule fileModule = new FileModule();

                var data = this.DB.檔案.Where(o => o.對應索引 == ID && o.對應名稱.StartsWith("SMS") && o.檔案模式 == "M" && o.檔案格式 == "F").FirstOrDefault();
                JsonParam jp = new JsonParam();

                if (data == null)
                {
                    jp.success = false;
                    throw new Exception("[簡訊維護啟動發送]於DB中找不到ID相對應的Excel檔案，ID為:" + ID );
                }


                XLWorkbook excelWB = new XLWorkbook(string.Concat(rootPath,data.檔案路徑));
                IXLWorksheet sheet = excelWB.Worksheet(1);
                int RowCount = sheet.RowsUsed().Count();
                IList<SMSDetail> sm = new List<SMSDetail>();
                try
                {
                    for (int i = 2; i <= RowCount; i++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 1).Value)))
                        {
                            sm.Add(new SMSDetail { SMS_ID = ID, NAME = sheet.Cell(i, 2).Value.ToString(), TEL = sheet.Cell(i, 1).Value.ToString(),USEDFOR = 1, INSERT_DATE = DateTime.UtcNow.AddHours(8), INSERT_NAME = UserProvider.Instance.User.ID });
                        };
                    }

                    if(sm.Count <= 0)
                    {
                        jp.errorMsg = "[簡訊維護啟動發送]DB儲存至SMSDetail的List為空值";
                        jp.success = false;
                        return jp;
                    }

                    this.DB.SMSDetail.AddRange(sm);
                    this.DB.SaveChanges();
                    jp.success = true;
                    jp.list = sm.ToList();
                    addList = sm.ToList(); //暫存目前增加的List
                    return jp;
                }

                catch (Exception ex)
                {
                    jp.success = false;
                    jp.errorMsg = "[簡訊維護啟動發送] 讀取Excel時錯誤，錯誤如下:" + ex;
                    return jp;
                }
            }
            else
            {
                //測試用
                JsonParam jp = new JsonParam();
                IList<SMSDetail> sm = new List<SMSDetail>();
                try
                {
                 
                    if (!string.IsNullOrEmpty(Convert.ToString(testTel)))
                    {
                        sm.Add(new SMSDetail { SMS_ID = ID,NAME = testName, TEL = testTel, USEDFOR = 0, INSERT_DATE = DateTime.UtcNow.AddHours(8), INSERT_NAME = UserProvider.Instance.User.ID});
                    };
              
                    this.DB.SMSDetail.AddRange(sm);
                    this.DB.SaveChanges();
                    jp.success = true;
                    jp.list = sm.ToList();
                    addList = sm.ToList(); //暫存目前增加的List
                    return jp;
                }
                catch(Exception ex)
                {
                    jp.success = false;
                    jp.errorMsg = "[簡訊維護啟動測試用發送]儲存測試人員發生錯誤，錯誤: " + ex.Message;
                    return jp;
                }
            }
        }
        #endregion

        #region 查詢餘額
        /// <summary>
        /// 查詢三竹簡訊餘額
        /// </summary>
        public int GetAccount()
        {

            MitakeSMSClass ms = GetSMSClassEntity();

            Task<MitakeSMSClass.AccountParam> rp;

            rp = ms.SearchSMSAccount();

            if (rp.Result.state == 0)
            {
                return rp.Result.account;
            }
            else
            {
                throw new Exception("[簡訊帳戶餘額搜尋] 呼叫MitakeSMS.dll SearchSMSAccount()，dll內部回傳錯誤訊息:" + rp.Result.errorMsg);
            }

        }
        #endregion

        #region 儲存從Response回傳的資料
        /// <summary>
        /// 儲存三竹簡訊回傳至Response的資料
        /// </summary>
        /// <param name="sp">MitakeSMSClass.SaveResponseParam</param>
        public Boolean SaveSMSResponseToDB(MitakeSMSClass.SaveResponseParam sp)
        {
            var success = false;
            MitakeSMSClass ms = GetSMSClassEntity();

            if(sp == null)
            {
                throw new Exception("[簡訊Response存入DB]沒有接收到三竹所回傳的值");
            }

            try
            {
                ms.SaveNotify(sp); //呼叫dll，將資料存入SMSResponseMsg資料表

                var data = this.DB.SMSDetail.Where(x => x.MSG_ID == sp.msgid).FirstOrDefault();

                if (data != null)
                {
                    data.STATUSCODE_RE = sp.statuscode;
                    data.STATUSFLAG_RE = sp.statusFlag;
                    data.STATUSSTR_RE = sp.statusstr;
                    data.RECEIVE_DATE_RE = sp.receiveDate;
                    data.STATUSSTRMEMO_RE = statusstrResDic[sp.statusstr];
                    data.DLVTIME_RE = sp.dlvtime;
                    data.DONETIME_RE = sp.donetime;
                }

                this.DB.SaveChanges();
                success = true;
                return success;

            }catch(Exception e)
            {
                throw new Exception("[簡訊Response存入DB]儲存DB時發生錯誤，錯誤:" + e.Message);
            }
        }
        #endregion

        #region 取得三竹及時回傳的訊息
        /// <summary>
        /// 取得三竹及時回傳的訊息
        /// </summary>
        /// <param name="ID">ID</param>
        [HttpPost]
        public List<SMSDetail> GetImmediateReturn(int ID)
        {

            var result = new List<SMSDetail>();

            var data = this.DB.SMSDetail.Where(w => w.SMS_ID == ID).ToList();
            if (data != null)
            {
                foreach (var d in data)
                {
                    if (d.STATUSSTR_RE != null)
                    {
                        //若Response有回傳值
                        result.Add(new SMSDetail { SMS_ID = d.SMS_ID, NAME = d.NAME, TEL = d.TEL, STATUSCODE = d.STATUSSTRMEMO_RE,USEDFOR = d.USEDFOR });
                    }
                    else
                    {
                        if (d.IS_SUCCESS == false)
                        {
                            //若此資料在執行的過程中有發生錯誤
                            result.Add(new SMSDetail { SMS_ID = d.SMS_ID, NAME = d.NAME, TEL = d.TEL, STATUSCODE = "啟動發送時發生錯誤", USEDFOR = d.USEDFOR });
                        }
                        else
                        {
                            result.Add(new SMSDetail { SMS_ID = d.SMS_ID, NAME = d.NAME, TEL = d.TEL, STATUSCODE = d.STATUSCODE, USEDFOR = d.USEDFOR });
                        }
                    }

                }
            }
            return result;

        }

        #endregion

        #region 取消預約
        /// <summary>
        /// 取消三竹簡訊預約
        /// </summary>
        /// <param name="ID">ID</param>
        [HttpPost]
        public int CancelReservation(int ID)
        {
            var SMSData = this.DB.SMS.Where(x => x.SMS_ID == ID).FirstOrDefault();
            if (SMSData == null)
            {
                throw new Exception("[簡訊取消預約] SMS找不到此筆ID紀錄，ID為:" + ID);
            }


            //將狀態改為傳入三竹中(預設為成功)
            SMSData.STATUSDETAIL = 9;


            string msgString = BuildCancelMsgString(ID);
            var count = 0;
            if(msgString == "")
            {
                throw new Exception("[簡訊取消預約]取消的MSGID字串為空值");
            }
            else
            {
                MitakeSMSClass ms = GetSMSClassEntity();

                Task<MitakeSMSClass.CancelReturnModel> cr;

                cr = ms.CancelReserveSMS(msgString);
                count = cr.Result.cancelData.Count();

                if (cr.Result.state == 0)
                {
                    if (count > 0) 
                    {
                        SMSData.CANCEL_DATE = DateTime.UtcNow.AddHours(8);
                        SMSData.CANCEL_NAME = UserProvider.Instance.User.ID;
                        SMSData.STATUS = 3; //預約取消      
                        SMSData.STATUSDETAIL = 11; //將狀態改為三竹回傳成功

                        foreach (var c in cr.Result.cancelData)
                        {
                            var data = this.DB.SMSDetail.Where(x => x.MSG_ID == c.msgId).FirstOrDefault();
                            if (data != null)
                            {
                                data.STATUSCODE = c.status;
                            }

                        }
                        try
                        {
                            this.DB.SaveChanges();

                        }
                        catch (Exception e)
                        {
                            throw new Exception("[簡訊取消預約] DB儲存dll回傳訊息錯誤" + e.Message);
                        }
                    }
                }
                else
                {
                    if(cr.Result.state == -2)
                    {
                        //將狀態改為三竹回傳失敗
                        SMSData.STATUSDETAIL = 12;
                    }

                    if (cr.Result.state == -1)
                    {
                        //將狀態改為傳入三竹失敗
                        SMSData.STATUSDETAIL = 10;
                    }
                    throw new Exception("[簡訊取消預約] 呼叫MitakeSMS.dll CancelReserveSMS()，dll內部回傳錯誤訊息" + cr.Result.errorMsg);
                }
            }

            return count; //回傳預約取消的人員數量;
        }
        #endregion

        #region 組取消預約的MSGID字串
        /// <summary>
        /// 組取消預約的MSGID字串
        /// </summary>
        /// <param name="ID">ID</param>
        public string BuildCancelMsgString(int ID)
        {
            string cancelString = "";
            int cancelNum = 0;
            var data = this.DB.SMSDetail.Where(w => w.SMS_ID == ID && w.USEDFOR == 1).Select(x => x.MSG_ID).ToList();

            if (data.Count > 0)
            {
                var dc = data.Count();

                foreach (var d in data)
                {
                    if (d != null)
                    {
                        cancelNum++;

                        if (dc == cancelNum)
                        {
                            cancelString += d;
                        }
                        else
                        {
                            cancelString += d + ",";
                        }
                    }
                }
            }
            else
            {
                throw new Exception("[簡訊取消預約]SMSDetail找不到ID相對應的資料，ID為:" + ID);
            }

            return cancelString;
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


        #endregion


        public class JsonParam
        {
            public bool success { get; set; } //是否成功
            public string errorMsg { get; set; } //錯誤訊息
            public List<SMSDetail> list { get; set; } 
            public int count { get; set; } //傳送的人員數
        }

        public class ReturMsgParam
        {
            public bool success { get; set; } //是否成功
            public string errorMsg { get; set; } //錯誤訊息
            public int identityId { get; set; } //主檔ID
            public int count { get; set; } //送出人數
        }


        Dictionary<string, string> statusstrResDic = new Dictionary<string, string>() {
            {"DELIVRD","已送達手機" },
            {"EXPIRED","逾時無法送達" },
            {"DELETED","預約已取消" },
            {"UNDELIV","無法送達(門號有錯誤/簡訊已停用)" },
            {"ACCEPTD","簡訊處理中" },
            {"UNKNOWN","無效的簡訊狀態，系統有錯誤" },
            {"SYNTAXE","簡訊內容有錯誤" },
            {"REJECTD","" },

        };

    }
}