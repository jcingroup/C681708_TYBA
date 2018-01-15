using OutWeb.Entities;
using OutWeb.Enums;
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
using System.Configuration;
using ClosedXML.Excel;
using System.IO;
using System.Text;
using SFTPConnect;
using NLog;

namespace OutWeb.Modules.Manage
{
    /// <summary>
    /// 傳真列表模組
    /// </summary>
    public class FaxModule : ListModuleService
    {
        private DBEnergy m_DB = new DBEnergy();
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        private IList<FAXDetail> addList = null;
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public override void DoDeleteByID(int ID)
        {
            var data = this.DB.FAX.Where(s => s.FAX_ID == ID).FirstOrDefault();
            if (data == null)
            {
                logger.Info("[刪除傳真] 查無此傳真，可能已被移除"); //將錯誤記在NLog裡
                throw new Exception("查無此傳真，可能已被移除");
            }

            if (data.STATUS != 0) //非草稿不得刪除
            {
                logger.Info("[刪除傳真]刪除傳真錯誤，因為此傳真為非草稿狀態，ID:" + ID); //將錯誤記在NLog裡
                throw new Exception("非'未啟動'狀態則無法進行刪除");
            }

            //刪除上傳的檔案
            var findFile = this.DB.檔案.Where(x => x.對應索引 == ID && x.檔案模式 == "M" && x.檔案格式 == "F" && x.對應名稱 == "Fax").FirstOrDefault();

            if (findFile != null)
            {
                this.DB.檔案.Remove(findFile);
                File.Delete(string.Concat(rootPath, findFile.檔案路徑));
            }
            try
            {
                this.DB.FAX.Remove(data);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Info("[刪除傳真]刪除錯誤，錯誤訊息:" + ex.Message); //將錯誤記在NLog裡
                throw new Exception("傳真刪除發生錯誤");
            }
        }

        public override object DoGetDetailsByID(int ID)
        {
            FaxDataModel result = new FaxDataModel();
            FAX data = DB.FAX.Where(w => w.FAX_ID == ID).FirstOrDefault();
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
            return 0;
        }


        public ReturnMsgParam DoSaveAndExecData(FormCollection form, Language language, int? ID = null, List<HttpPostedFileBase> filesMember = null, List<HttpPostedFileBase> filesAttachment = null)
        {
            FAX saveModel;
            ImageRepository imgepository = new ImageRepository();
            FileRepository fileRepository = new FileRepository();
            JsonParam jp = new JsonParam();
            ReturnMsgParam rm = new ReturnMsgParam();

            #region 更新資料(更新或新增)
            if (!ID.HasValue || ID == 0)
            {
                saveModel = new FAX();
                saveModel.INSERT_NAME = UserProvider.Instance.User.ID;
                saveModel.INSERT_DATE = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                saveModel = this.DB.FAX.Where(s => s.FAX_ID == ID).FirstOrDefault();
                saveModel.UPDATE_NAME = UserProvider.Instance.User.ID;
                saveModel.UPDATE_DATE = DateTime.UtcNow.AddHours(8);

            }

            saveModel.TITLE = form["TITLE"];
            saveModel.PUBLISHER = form["PUBLISHER"];
            saveModel.DELIEVER_TYPE = form["DELIEVER_TYPECheck"];
            saveModel.FAX_TITLE = form["FAX_TITLE"];
            saveModel.FILE_PAGE = Convert.ToInt16(form["FILE_PAGE"]);
            PublicMethodRepository.FilterXss(saveModel);

            if (form["DELIEVER_TYPECheck"] == "R")
            {
                string date = form["DELIEVER_DATE_DATE"] + " " + form["DELIEVER_DATE_TIME"] + ":00";
                DateTime myDate = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
                DateTime dt = Convert.ToDateTime(date);
                saveModel.DELIEVER_DATE = dt;
            }
            else
            {
                saveModel.DELIEVER_DATE = System.DateTime.Now;
            }

            saveModel.STATUS = 0; //儲存草稿

            if (ID.HasValue && ID != 0)
            {
                this.DB.Entry(saveModel).State = EntityState.Modified;
            }
            else
            {
                this.DB.FAX.Add(saveModel);
            }

            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }


            int identityId = (int)saveModel.FAX_ID;


            #region 檔案處理 聯絡人清單

            FileSaveHandler(form, identityId, "MemberFile", "Fax", filesMember, FileUploadType.MODE1);

            #endregion 檔案處理 聯絡人清單

            #region 檔案處理 附件檔

            FileSaveHandler(form, identityId, "AttachmentFile", "Fax", filesAttachment, FileUploadType.MODE2);

            #endregion 檔案處理 附件檔

            #endregion

            #region 寄送HiNetFax傳真
            if (form["type"] == "send" || form["type"] == "test")
            {

                if (form["type"] == "send")
                {
                    jp = ReadExcelContent(identityId, form["type"], null, null,null); //製作til檔案
                }
                else
                {
                    jp = ReadExcelContent(identityId, form["type"], form["SENDTESTTEL"], form["SENDTESTNAME"],form["SENDTESTCOMPANY"]); //製作til檔案
                }

                if (jp.success == true)
                {
                    jp = ConvertTILEncodingAndSaveFile(jp.faxContent, identityId); //轉檔和複製檔案並且傳至SFTP

                    if (jp.success == true)
                    {
                        if (form["type"] == "send")
                        {
                            saveModel.STATUS = 1;//將狀態改為立即啟動
                            this.DB.SaveChanges();
                        }

                        rm.success = true;
                    }
                    else
                    {
                        rm.success = false;
                        rm.showCustomerMsg = jp.showCustomerMsg;
                        logger.Info(jp.errorMsg); //將錯誤記在NLog裡
                    }
                }
                else
                {
                    rm.success = false;
                    rm.showCustomerMsg = jp.showCustomerMsg;
                    logger.Info(jp.errorMsg); //將錯誤記在NLog裡
                }

            }
            #endregion


            //將是否執行成功的狀態回寫到FAXDetail
            if (addList != null)
            {
                foreach (var data in addList)
                {
                    var dd = this.DB.FAXDetail.Where(x => x.FAX_NO == data.FAX_NO).FirstOrDefault();
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


        /// <summary>
        /// 讀取Excel檔案且組til檔所需內容
        /// </summary>
        /// <param name="ID">ID</param>
        /// <param name="type">傳送類型(test、send)></param>
        /// <param name="testTel">測試傳真號碼</param>
        /// <param name="testName">測試收件者姓名</param>
        /// <param name="testCompany">測試收件者公司</param>
        private JsonParam ReadExcelContent(int ID, string type, string testTel, string testName,string testCompany)
        {
            JsonParam jp = new JsonParam();
            //取檔案
            FileModule fileModule = new FileModule();
            IXLWorksheet sheet = null;

            int RowCount = 0;

            //正式檔才需要帶入Excel檔案
            if (type != "test")
            {
                var fileMemberData = this.DB.檔案.Where(o => o.對應索引 == ID && o.對應名稱.StartsWith("Fax") && o.檔案模式 == "M" && o.檔案格式 == "F" && o.識別碼 == (int)FileUploadType.MODE1).FirstOrDefault();

                if (fileMemberData == null)
                {
                    jp.success = false;
                    jp.errorMsg = "[傳真維護啟動發送]於DB中找不到ID相對應的Excel檔案，ID為:" + ID;
                    jp.showCustomerMsg = "找不到收件人的上傳檔案";
                    return jp;
                }

                XLWorkbook excelWB = new XLWorkbook(string.Concat(rootPath,fileMemberData.檔案路徑));
                sheet = excelWB.Worksheet(1);
                RowCount = sheet.RowsUsed().Count();

            }

            var fileAttachmentData = this.DB.檔案.Where(o => o.對應索引 == ID && o.對應名稱.StartsWith("Fax") && o.檔案模式 == "M" && o.檔案格式 == "F" && o.識別碼 == (int)FileUploadType.MODE2).FirstOrDefault();

            if (fileAttachmentData == null)
            {
                jp.success = false;
                jp.errorMsg = "[傳真維護啟動發送]於DB中找不到ID相對應的附件檔案，ID為:" + ID;
                jp.showCustomerMsg = "找不到附件的上傳檔案";
                return jp;
            }

            var faxData = this.DB.FAX.Where(x => x.FAX_ID == ID).FirstOrDefault();

            if (faxData == null)
            {
                jp.success = false;
                jp.errorMsg = "[傳真維護啟動發送]於DB中找不到ID相對應的FAX資料，ID為:" + ID;
                jp.showCustomerMsg = "找不到此筆傳真檔案紀錄";
                return jp;
            }


            string fileType = "";

            #region HiNetFax TIL內容組合
            string file_ext = Path.GetExtension(fileAttachmentData.原始檔名); //取得檔案副檔名

            //檔案類型
            if (file_ext.ToLower().Contains(".docx"))
            {
                fileType = "docx";
            }
            else if (file_ext.ToLower().Contains(".doc"))
            {
                fileType = "doc";
            }
            else
            {
                fileType = "pdf";
            }

            string COMPANY = ConfigurationManager.AppSettings["FaxCompany"];//寄件人公司名稱
            string CODE = ConfigurationManager.AppSettings["FaxCode"]; //表頭字碼
            string REPLY = ConfigurationManager.AppSettings["FaxReplyEmail"];  //傳送結果回報之eMail接收帳號
            string FAX = ConfigurationManager.AppSettings["FaxNo"]; //寄件人傳真號碼
            string NAME = faxData.PUBLISHER; //寄件人


            string SUBJECT = faxData.FAX_TITLE; //自行定義不能超過20字
            string PAGES = faxData.FILE_PAGE.ToString();
            string REPORT = ConfigurationManager.AppSettings["FaxReport"]; // (1=不要回報,2=只回報失敗,3=全部回報,4 = 批次回報)
            string BRODCAST=""; //一對多時brocast 設 1;單獨一個受話號碼 brocast 設 0
            string FORMAT = ""; //傳真圖檔之格式  brocast 設 0
            string YRES = ""; //解析度 固定 (轉圖檔時需一致)
            string SEQNO = ""; //續號
            string NO = ""; //收件者的傳真號碼
            string RNAME = ""; //收件者的姓名
            string RCOMPANY = ""; //收件者的公司
            string COMBINE = "";

            //附件檔案類型
            if (fileType == "doc")
            {
                FORMAT = "FORMAT:DOC" + Convert.ToString((char)10);
                YRES = "YRES:196" + Convert.ToString((char)10);
            }
            else if (fileType == "docx")
            {
                FORMAT = "FORMAT:DOCX" + Convert.ToString((char)10);
                YRES = "YRES:196" + Convert.ToString((char)10);
            }
            else
            {
                FORMAT = "FORMAT:PDF" + Convert.ToString((char)10);
                YRES = "YRES:98" + Convert.ToString((char)10);
            }


            if (type == "test")
            {
                //代表為測試用
                BRODCAST = "0";
                NO = "NO:" + testTel + Convert.ToString((char)10);
                SEQNO = "SEQNO:0" + Convert.ToString((char)10);
                RNAME = "RNAME:" + testName + Convert.ToString((char)10);
                RCOMPANY = "COMPANY:" + testCompany + Convert.ToString((char)10);
                COMBINE += NO + SEQNO + RNAME + RCOMPANY;
            }
            else
            {
                if (RowCount == 2)
                {
                    //代表只有一個成員
                    BRODCAST = "0";
                    NO = "NO:" + Convert.ToString(sheet.Cell(2, 1).Value) + Convert.ToString((char)10);
                    SEQNO = "SEQNO:0" + Convert.ToString((char)10); ;
                    RNAME = "RNAME:" + Convert.ToString(sheet.Cell(2, 2).Value).ToString() + Convert.ToString((char)10);
                    RCOMPANY = "RCOMPANY:" + Convert.ToString(sheet.Cell(2, 3).Value).ToString() + Convert.ToString((char)10);
                    COMBINE += NO + SEQNO + RNAME + RCOMPANY;
                }
                else
                {
                    //跳過Title
                    for (int i = 2; i <= RowCount; i++) 
                    {
                        BRODCAST = "1";
                        NO = "NO:" + Convert.ToString(sheet.Cell(i, 1).Value) + Convert.ToString((char)10);
                        SEQNO = "SEQNO:" + (i - 1).ToString() + Convert.ToString((char)10);
                        RNAME = "RNAME:" + Convert.ToString(sheet.Cell(i, 2).Value).ToString() + Convert.ToString((char)10);
                        RCOMPANY = "RCOMPANY:" + Convert.ToString(sheet.Cell(i, 3).Value).ToString() + Convert.ToString((char)10);
                        COMBINE += NO + SEQNO + RNAME + RCOMPANY;
                    }

                }
            }



            //HiNetFax需要內容
            string faxContent = "";
            faxContent += "SERVICETYPE:25" + Convert.ToString((char)10); //傳送模式，請設為25
            faxContent += "REPLY:" + REPLY + Convert.ToString((char)10);
            faxContent += FORMAT;
            faxContent += "PAGES:" + PAGES + Convert.ToString((char)10);
            faxContent += "PAGESIZE:1" + Convert.ToString((char)10); //固定為1 
            faxContent += "ORIENTATION:Plain" + Convert.ToString((char)10);
            faxContent += "XRES:204" + Convert.ToString((char)10); //解析度 固定(轉圖檔時需一致)
            faxContent += YRES;
            faxContent += "WIDTH:2152" + Convert.ToString((char)10);
            faxContent += "LENGTH:2851" + Convert.ToString((char)10);
            faxContent += "NAME:" + NAME + Convert.ToString((char)10);
            faxContent += "FAX:" + FAX + Convert.ToString((char)10);
            faxContent += "COMPANY:" + COMPANY + Convert.ToString((char)10);
            faxContent += "CODE:" + CODE + Convert.ToString((char)10);
            faxContent += "SUBJECT:" + SUBJECT + Convert.ToString((char)10);
            faxContent += "REPORT:" + REPORT + Convert.ToString((char)10);
            faxContent += "BROCAST:" + BRODCAST + Convert.ToString((char)10);
            faxContent += COMBINE;

            #endregion HiNetFax內容組合


            if (faxContent == "")
            {
                jp.success = false;
                jp.errorMsg = "[傳真維護啟動發送]組成傳真內容的FaxContent為空值";
                jp.showCustomerMsg = "產生內部檔案時發生錯誤";
                return jp;
            }

            jp.faxContent = faxContent;

            #region 將檔案資料紀錄於DB
            if (type == "send")
            {
                IList<FAXDetail> fd = new List<FAXDetail>();

                try
                {
                    for (int i = 2; i <= RowCount; i++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 1).Value)))
                        {
                            fd.Add(new FAXDetail { FAX_ID = ID, FAX_TITLE = faxData.TITLE, FAX_TEL = sheet.Cell(i, 1).Value.ToString(), FAX_COMPANY = sheet.Cell(i, 2).Value.ToString(), FAX_NAME = sheet.Cell(i, 3).Value.ToString(), SEND_COMPANY = COMPANY, SEND_NAME = NAME, USEDFOR = 1, INSERT_DATE = DateTime.UtcNow.AddHours(8), INSERT_NAME = UserProvider.Instance.User.ID });
                        };
                    }
                    this.DB.FAXDetail.AddRange(fd);
                    this.DB.SaveChanges();
                    jp.success = true;
                    addList = fd; //暫存目前增加的List

                }
                catch (Exception ex)
                {
                    jp.success = false;
                    jp.errorMsg = "[傳真維護啟動發送] 存取Excel檔案內容至FAXDETAIL TABLE時錯誤，錯誤如下:" + ex;
                    jp.showCustomerMsg = "儲存資料至資料庫時發生錯誤";
                    return jp;
                }
            }
            else
            {
                //測試用
                IList<FAXDetail> fd = new List<FAXDetail>();
                try
                {

                    fd.Add(new FAXDetail { FAX_ID = ID, FAX_TITLE = faxData.TITLE, FAX_TEL = testTel, FAX_COMPANY = testCompany, FAX_NAME = testName, SEND_NAME = NAME,SEND_COMPANY = COMPANY, USEDFOR = 0, INSERT_DATE = DateTime.UtcNow.AddHours(8), INSERT_NAME = UserProvider.Instance.User.ID });

                    this.DB.FAXDetail.AddRange(fd);
                    this.DB.SaveChanges();
                    jp.success = true;
                    addList = fd; //暫存目前增加的List

                }
                catch (Exception ex)
                {
                    jp.success = false;
                    jp.errorMsg = "[傳真維護啟動發送] 存取Excel檔案內容至FAXDETAIL TABLE時錯誤，錯誤如下:" + ex;
                    jp.showCustomerMsg = "儲存資料至資料庫時發生錯誤";
                    return jp;
                }

            }
            #endregion  
            return jp;

        }

        /// <summary>
        /// 將TIL檔轉成ANSI檔案類型，且存至檔案Table
        /// </summary>
        /// <param name="faxContent">TIL內容</param>
        /// <param name="ID">ID></param>
        public JsonParam ConvertTILEncodingAndSaveFile(String faxContent, int ID)
        {
            JsonParam p = new JsonParam();

            try
            {
                //刪除舊的TIL檔----
                var findExitTIL = this.DB.檔案.Where(x => x.對應索引 == ID && x.對應名稱 == "Fax" && x.檔案模式 == "M" && x.檔案格式 == "F" && x.識別碼 == (int)FileUploadType.MODE3).FirstOrDefault();
                if (findExitTIL != null)
                {
                    this.DB.檔案.Remove(findExitTIL);
                    if (File.Exists(string.Concat(rootPath, findExitTIL.檔案路徑)))
                    {
                        File.Delete(string.Concat(rootPath, findExitTIL.檔案路徑)); //若檔案在則進行刪除
                    }
                }
                //-----------------
            }catch(Exception ex)
            {
                p.success = false;
                p.errorMsg = "[傳真維護啟動發送] 刪除舊的TIL時發生錯誤，錯誤:" + ex;
                p.showCustomerMsg = "更新檔案時發生錯誤";
                return p;
            }


            FileRepository fileRepository = new FileRepository();
            string FAXUser = ConfigurationManager.AppSettings["FAXUser"];

            Encoding ascii = Encoding.GetEncoding("big5");//Encoding.ASCII;
            Encoding unicode = Encoding.Unicode;

            byte[] ansiBytes = ascii.GetBytes(faxContent);
            byte[] unicodeBytes = Encoding.Convert(ascii, unicode, ansiBytes);

            char[] unicodeChars = new char[unicode.GetCharCount(unicodeBytes, 0, unicodeBytes.Length)];
            unicode.GetChars(unicodeBytes, 0, unicodeBytes.Length, unicodeChars, 0);
            string strOutput = new string(unicodeChars);
            //Encoding ecp950 = Encoding.GetEncoding(950);
            //File.WriteAllText(@"C:\Users\Apple\Desktop\HN74355953.171121.til", strOutput, ecp950);


            DateTime now = DateTime.Now;
            //String NowStr = "";
            String YMDStr = "";
            String TimeStr = "";
            YMDStr = now.Year.ToString() + AddZero(now.Month.ToString()) + AddZero(now.Day.ToString()); //取日期 e.g.20171219
            TimeStr = AddZero(now.Hour.ToString()) + AddZero(now.Minute.ToString()) + AddZero(now.Second.ToString()); //時間格式 e.g.040932
            string tilNeedStr = "HN" + FAXUser + "." + TimeStr + ".til"; //檔名格式 HNNo.序號.tif (序號自訂，最多6碼) e.g.HN90000376.061122.til

            try
            {
                var mv = fileRepository.SaveTILFile(tilNeedStr, strOutput, 950);

                檔案 fileData = new 檔案();
                fileData.對應索引 = ID;
                fileData.對應名稱 = "Fax";
                fileData.檔案模式 = "M";
                fileData.檔案格式 = "F";
                fileData.檔案名稱 = mv.FileName;
                fileData.原始檔名 = mv.RealFileName;
                fileData.檔案路徑 = mv.FilePath;
                fileData.檔案虛擬路徑 = mv.FileUrl;
                fileData.建立人員 = UserProvider.Instance.User.ID;
                fileData.建立時間 = DateTime.UtcNow.AddHours(8);
                fileData.排序 = 0;
                fileData.識別碼 = (int)FileUploadType.MODE3;
                fileData.更新時間 = DateTime.UtcNow.AddHours(8);
                this.DB.檔案.Add(fileData);
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                p.success = false;
                p.errorMsg = "[傳真維護啟動發送] 創建完TIL檔，並且更新內容至'檔案'時錯誤，錯誤如下:" + ex;
                p.showCustomerMsg = "儲存資料至資料庫時發生錯誤";
                return p;
                //throw new Exception("[傳真維護啟動發送] 更新內容至'檔案'時錯誤，錯誤如下:" + ex);
            }

            var throwFile = ThrowFileToSFTP(ID, FAXUser, TimeStr, YMDStr);

            if (throwFile.success == false)
            {
                p.success = false;
                p.errorMsg = throwFile.errorMsg;
                return p;
            }

            p.success = true;
            return p;

        }


        /// <summary>
        /// 透過SFTPConnect.dll上傳傳真檔案
        /// </summary>
        /// <param name="ID">ID></param>
        /// <param name="UserName">Hinet帳號</param>
        /// <param name="timeStr">時間字串(6碼)</param>
        /// <param name="dateStr">年份日期字串</param>
        public JsonParam ThrowFileToSFTP(int ID, String UserName, String timeStr,String dateStr)
        {
            JsonParam jp = new JsonParam();
            FileRepository fileRepository = new FileRepository();
            List<FileParam> fp = new List<FileParam>();

            var findFaxData = this.DB.檔案.Where(x => x.對應索引 == ID && x.對應名稱 == "FAX" && x.檔案模式 == "M" && x.檔案格式 == "F" && (x.識別碼 == (int)FileUploadType.MODE2 || x.識別碼 == (int)FileUploadType.MODE3)).ToList();

            fp.Clear();
            var successCount = 0; //紀錄成功的檔案筆數

            foreach (var data in findFaxData)
            {
                var copyData = fileRepository.CopyFile(string.Concat(rootPath,data.檔案路徑), data.原始檔名, UserName, timeStr,dateStr); //執行檔案複製

                if (copyData.success == true)
                {
                    var va = findFaxData.Where(x => x.原始檔名 == copyData.oriName).FirstOrDefault();
                    if (va != null)
                    {
                        va.傳真檔案名稱 = copyData.fileName;
                        va.傳真檔案路徑 = copyData.filePath;
                        fp.Add(new FileParam { fileName = copyData.fileName, filePath = copyData.filePath });
                    }
                    successCount++;
                }
                else
                {
                    jp.success = false;
                    jp.errorMsg = copyData.errorMsg;
                    return jp;
                }
            }


            string SFTPConnect = ConfigurationManager.AppSettings["SFTPConnect"];
            int SFTPPort = Convert.ToInt16(ConfigurationManager.AppSettings["SFTPPort"]);
            string SFTPUser = UserName;
            string SFTPPassword = "sftpfax" + SFTPUser;

            if (fp.Count != 2)
            {
                jp.errorMsg = "[傳真維護啟動發送]不符合所要檔案數目2，偵測到預傳至SFTP檔案數為:" + fp.Count;
                jp.showCustomerMsg = "內部產生檔案時發生錯誤";
                jp.success = false;
                return jp;
            }

            #region 傳遞檔案至SFTP
            try
            {
                SFTPClass sftp = new SFTPClass(SFTPConnect, SFTPPort, SFTPUser, SFTPPassword);
                sftp.Connect();
                foreach (var v in fp)
                {
                    var absolutePosition = string.Concat(rootPath, v.filePath);
                    sftp.Put(absolutePosition, "/doc/" + v.fileName);
                    logger.Info("上傳檔案至SFTP 名稱:" + v.fileName); //紀錄上傳的檔案名稱
                    //File.Delete(v.filePath);//傳完刪除檔案
                }
                sftp.Disconnect();
            }
            catch (Exception e)
            {
                jp.errorMsg = "[傳真維護啟動發送]上傳至SFTP時發生錯誤，錯誤:" + e.ToString();
                jp.showCustomerMsg = "檔案傳至中華電信傳真系統時發生錯誤，系統可能處於忙碌中，請稍後再試...";
                jp.success = false;
                return jp;
            }

            #endregion


            try
            {
                this.DB.SaveChanges(); //有成功上傳至SFTP才回寫資料位置到FAXDetail Table
            }
            catch (Exception ex)
            {
                jp.success = false;
                jp.errorMsg = "[傳真維護啟動發送]檔案複製資料存至DB時發生錯誤，錯誤:" + ex.ToString();
                jp.showCustomerMsg = "儲存資料時至資料庫時發生錯誤";
                return jp;
            }

            jp.success = true;
            return jp;

        }

        /// <summary>
        /// 符合日期格式e.g.20171122
        /// </summary>
        /// <param name="s">日期字串</param>
        private string AddZero(string s)
        {
            string back = "";
            if (Convert.ToInt16(s) < 10)
            {
                back = "0" + s;
            }
            else
            {
                back = s;
            }
            return back;
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


        public class JsonParam
        {
            public bool success { get; set; } //是否成功
            public string errorMsg { get; set; } //錯誤訊息
            public string faxContent { get; set; } //傳真內容
            public string showCustomerMsg { get; set; }//給使用者看的錯誤訊息
            public List<int> NoList { get; set; }
        }

        public class FileParam
        {
            public string fileName { get; set; }
            public string filePath { get; set; }
        }

        public class ReturnMsgParam
        {
            public bool success { get; set; } //是否成功
            public string errorMsg { get; set; } //錯誤訊息
            public string showCustomerMsg { get; set; }//給使用者看的錯誤訊息
            public int identityId { get; set; } //主檔ID
            public int count { get; set; } //送出人數
        }

    }
}