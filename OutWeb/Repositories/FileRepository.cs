using ClosedXML.Excel;
using OutWeb.Models.Manage;
using OutWeb.Models.Manage.ExportExcelModels;
using OutWeb.Models.Manage.ExportExcelModels.QuestionnaireStatisticsReplyModels;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Modules.Manage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace OutWeb.Repositories
{
    public class FileRepository
    {
        private string savePath { get; set; }
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        public FileRepository()
        {
        }

        public FileRepository(string savePath)
        {
            this.savePath = savePath;
        }

        #region 儲存處理函式

        /// <summary>
        ///
        /// </summary>
        /// <param name="vm"></param>
        public void SaveFileToDB(FilesModel vm)
        {
            FileModule fileModule = new FileModule();
            fileModule.SaveFiles(vm);
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="myFile"></param>
        public void UploadFile(string uploadType, FilesModel vm, List<HttpPostedFileBase> files, string mode)
        {

            string serverMapPath = string.Empty;

            if (uploadType == "upload")
                serverMapPath = "/Content/Upload/Manage/Files/Temp/";
            else
                serverMapPath = "/Content/Upload/Manage/Files/";
            if (mode == "S")
            {
                foreach (var m in vm.MemberData)
                    m.FilePath = HttpContext.Current.Server.MapPath(serverMapPath + m.FileName).Replace(rootPath, "");
            }
            else
            {
                foreach (var m in vm.MemberDataMultiple)
                    m.FilePath = HttpContext.Current.Server.MapPath(serverMapPath + m.FileName).Replace(rootPath, "");
            }

            if (files != null && files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string strFileName = uploadType == "upload" ? files[i].FileName : Guid.NewGuid().ToString() + Path.GetExtension(files[i].FileName);
                    string strFilePath = HttpContext.Current.Server.MapPath(serverMapPath + strFileName);
                    string realFileName = files[i].FileName;
                    files[i].SaveAs(strFilePath);

                    #region data binding to model

                    if (mode == "S")
                    {
                        vm.MemberData.Add(new MemberViewModel()
                        {
                            RealFileName = realFileName,
                            FilePath = strFilePath.Replace(rootPath, ""),
                            FileName = strFileName,
                            FileUrl = serverMapPath.Substring(2, serverMapPath.Length - 2) + strFileName,
                        });
                    }
                    else if (mode == "M")
                    {
                        vm.MemberDataMultiple.Add(new MemberViewModel()
                        {
                            RealFileName = realFileName,
                            FilePath = strFilePath.Replace(rootPath, ""),
                            FileName = strFileName,
                            FileUrl = serverMapPath.Substring(1, serverMapPath.Length - 1) + strFileName,
                        });
                    }

                    #endregion data binding to model
                }
            }
        }

        #endregion 儲存處理函式

        #region Excel處理函式

        public MemoryStream ObjectToExcel<TModel>(TModel obj) where TModel : ReplyBase
        {
            Type type = obj.GetType();
            MemoryStream fs = new MemoryStream();
            var typeProcessorMap = new Dictionary<Type, Delegate>
            {
                    {
                        typeof(ReplyDataModel), new Action<ReplyDataModel>(m =>
                        {
                            ReplyDataModel model = (m as ReplyDataModel);
                            try
                                {
                                    /* Excel Bind*/
                                    var wb = new XLWorkbook();
                                    var wsht = wb.Worksheets.Add("Sheet1");
                                    wsht.ColumnWidth = 16;
                                    wsht.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    string[] wshtTitle =  new string[] {"回答日期" ,"姓名","帳號","題目類型","題目","答案"};
                                    //Title
                                    for (int i = 1; i < 7; i++)
                                        wsht.Cell(1,i).Value =wshtTitle[i-1];
                                    //Content
                                    for (int i = 0; i < model.Details.Count; i++)
                                    {
                                        wsht.Cell(i+2,1).Value = model.Details[i].AnswerDate;
                                        wsht.Cell(i+2,2).Value = model.Details[i].TopicAnswerUser;
                                        wsht.Cell(i+2,3).Value = model.Details[i].TopicAnswerID;
                                        wsht.Cell(i+2,4).Value = model.Details[i].TopicTypeName;
                                        wsht.Cell(i+2,5).Value = model.Details[i].TopicContent;
                                        if(model.Details[i].TopicAnswerItemNumber ==null)
                                        wsht.Cell(i+2,6).Value = model.Details[i].TopicAnswerTextContent;
                                        else
                                        wsht.Cell(i+2,6).Value = model.Details[i].TopicAnswerItemContent;
                                    }

                                    wb.SaveAs(fs);
                                    fs.Position = 0;
                                    fs.Close();
                                    fs.Dispose();
                                    wsht.Dispose();
                                    wb.Dispose();

                                }
                                catch (Exception ex)
                                {
                                model.IsSuccess = false;
                                model.Message = ex.Message;
                                }
                            model.IsSuccess =true;
                        })
                },
            { typeof(OutWeb.Models.Manage.ExportExcelModels.TrainSignListModels.ReplyDataModel), new Action<ReplyBase>(m => {
                /* Excel Bind*/
                OutWeb.Models.Manage.ExportExcelModels.TrainSignListModels.ReplyDataModel model =
                (m as OutWeb.Models.Manage.ExportExcelModels.TrainSignListModels.ReplyDataModel);
                try
                {
                    string folderPath = "~/Content/ExcelTemp/";
                    string TemplateFile =string.Empty;
                    int rangeDefualtLength = 0;
                    int index = 1;
                    switch (model.GetExcelFormType)
                    {
                        case ExcelForm.EmptyForm1:
                            TemplateFile=  HttpContext.Current.Server.MapPath(folderPath + "ApplySignEmptyList.xlsx");
                            rangeDefualtLength =7;
                        break;
                        case ExcelForm.EmptyForm2:
                            TemplateFile=  HttpContext.Current.Server.MapPath(folderPath + "ApplyEmptyList.xlsx");
                            rangeDefualtLength=5;
                        break;
                        default:
                            throw new Exception("can't get method with excel base file.");
                    }

                    XLWorkbook exl = new XLWorkbook(TemplateFile);
                    IXLWorksheet wsht = exl.Worksheet(1);
                    var defualtRowHeight = wsht.Row(2).Height;
                    var sexCell= wsht.Cell("G2").Value;
                    int satartRow = wsht.RowsUsed().Count()-1;

                    foreach (var data in model.Data)
                    {
                        for (int i = 0; i < data.ParticipantsData.Count; i++)
                        {
                            wsht.Cell(++satartRow,1).Value =index;
                            wsht.Cell(satartRow,2).Value = data.UserNo;
                            wsht.Cell(satartRow,3).Value = data.CompanyName;
                            wsht.Cell(satartRow,4).Value = data.ParticipantsData[i].Name;
                            wsht.Cell(satartRow,5).Value =data.ParticipantsData[i].JobTitle;
                            if(model.GetExcelFormType == ExcelForm.EmptyForm1)
                            wsht.Cell(satartRow,7).Value = sexCell;
                            wsht.Row(satartRow).Height = defualtRowHeight;

                              index++;
                        }
                    }

                        satartRow = wsht.RowsUsed().Count();
                        IXLRange range = wsht.Range(2, 1, satartRow, rangeDefualtLength);
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        exl.SaveAs(fs);
                        fs.Position = 0;
                        fs.Close();
                        fs.Dispose();
                        wsht.Dispose();
                        exl.Dispose();
                    }
                    catch (Exception ex)
                    {
                        model.IsSuccess = false;
                        model.Message = ex.Message;
                    }
                model.IsSuccess =true;
                }) },
                };

            typeProcessorMap[type].DynamicInvoke(obj);
            if (!obj.IsSuccess)
                throw new Exception(obj.Message);
            return fs;
        }

        #endregion Excel處理函式


        /// <summary>
        /// 用於HINETFAX所需的TIL文件
        /// </summary>
        /// <param name="filename">原始檔案名稱</param>
        /// <param name="strOutput">轉檔後TIL內容</param>
        /// <param name="encoding">編碼模式</param>
        public MemberViewModel SaveTILFile(String filename, String strOutput, int encoding)
        {
            string serverMapPath = string.Empty;
            serverMapPath = "~/Content/Upload/Manage/Files/";
            string strRealFileName = filename;
            string strFileName = Guid.NewGuid().ToString() + Path.GetExtension(filename);
            string strFilePath = HttpContext.Current.Server.MapPath(serverMapPath + strFileName).Replace(rootPath, "");

            string FileUrl = serverMapPath.Substring(2, serverMapPath.Length - 2) + strFileName;

           var mv = new MemberViewModel();

            mv.RealFileName = strRealFileName;
            mv.FilePath = strFilePath;
            mv.FileName = strFileName;
            mv.FileUrl = FileUrl;

            Encoding ecp = Encoding.GetEncoding(950);
            File.WriteAllText(string.Concat(rootPath,strFilePath), strOutput, ecp);

            return mv;
        }

        /// <summary>
        /// 將傳真所需的檔案格式另存成既定格式
        /// </summary>
        /// <param name="filePath">原始檔案路徑</param>
        /// <param name="oriName">原始檔案內容</param>
        /// <param name="hiNetUser">HINET帳戶名稱</param>
        /// <param name="timeStr">時間字串(6碼)</param>
        /// <param name="dateStr">年份日期字串</param>
        public Param CopyFile(String filePath, String oriName, String hiNetUser, String timeStr, String dateStr)
        {
            Param li = new Param();
            string serverMapPath = string.Empty;


            serverMapPath = "~/Content/Upload/Manage/Files/Fax/" + dateStr + "/"; //創建日期資料夾

            string tilNeedStr = "HN" + hiNetUser + "." + timeStr; //檔名格式 HNNo.序號.tif (序號自訂，最多6碼) e.g.HN90000376.911122.til

            string file_ext = Path.GetExtension(oriName); //取得檔案副檔名
            //檔案類型
            if (file_ext.ToLower().Contains(".docx"))
            {
                tilNeedStr += ".docx";
            }
            else if (file_ext.ToLower().Contains(".doc"))
            {
                tilNeedStr += ".doc";
            }
            else if (file_ext.ToLower().Contains(".pdf"))
            {
                tilNeedStr += ".pdf";
            }
            else
            {
                tilNeedStr += ".til";
            }

            string strFilePath = HttpContext.Current.Server.MapPath(serverMapPath); //檢查資料夾目錄是否存在

            if (!Directory.Exists(strFilePath))
            {
                //新增資料夾
                Directory.CreateDirectory(strFilePath);
            }

            strFilePath = HttpContext.Current.Server.MapPath(serverMapPath + tilNeedStr);  //產生CopyTo需要的路徑

            var relatePath = strFilePath.Replace(rootPath, ""); //取相對路徑
            try
            {
                FileInfo fi = new FileInfo(@filePath);
                fi.CopyTo(@strFilePath, true); // existing file will be overwritten
                li.success = true;
                li.fileName = tilNeedStr;
                li.filePath = relatePath;
                li.oriName = oriName;
            }
            catch (Exception ex)
            {
                li.success = false;
                li.errorMsg = "[傳真維護啟動發送] 複製檔案名稱為: " + oriName + " 時發生錯誤，錯誤訊息:" + ex.ToString();
            }
            return li;
        }

        public class Param
        {
            public bool success { get; set; }
            public string errorMsg { get; set; }
            public string fileName { get; set; }
            public string filePath { get; set; }
            public string oriName { get; set; }
        }

    }
}