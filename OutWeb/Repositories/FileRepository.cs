using ClosedXML.Excel;
using OutWeb.Models.Manage;
using OutWeb.Models.Manage.ApplyMaintainModels;
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

            foreach (var m in vm.MemberDataMultiple)
                m.FilePath = HttpContext.Current.Server.MapPath(serverMapPath + m.FileName).Replace(rootPath, "");

            if (files != null && files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string strFileName = uploadType == "upload" ? files[i].FileName : Guid.NewGuid().ToString() + Path.GetExtension(files[i].FileName);
                    string strFilePath = HttpContext.Current.Server.MapPath(serverMapPath + strFileName);
                    string realFileName = files[i].FileName;
                    files[i].SaveAs(strFilePath);

                    #region data binding to model

                    vm.MemberDataMultiple.Add(new FileViewModel()
                    {
                        RealFileName = realFileName,
                        FilePath = strFilePath.Replace(rootPath, ""),
                        FileName = strFileName,
                        FileUrl = serverMapPath.Substring(1, serverMapPath.Length - 1) + strFileName,
                    });

                    #endregion data binding to model
                }
            }
        }

        #endregion 儲存處理函式


        #region Excel處理函式

        public MemoryStream ObjectToExcel<TModel>(TModel obj) where TModel : ExcelReplyBase
        {
            Type type = obj.GetType();
            MemoryStream fs = new MemoryStream();
            var typeProcessorMap = new Dictionary<Type, Delegate>
            {
            { typeof(ApplyExcelReplyDataModel), new Action<ExcelReplyBase>(m => {
                /* Excel Bind*/
               ApplyExcelReplyDataModel  model =(m as ApplyExcelReplyDataModel);
                try
                {
                    string folderPath = "~/Content/ExcelTemp/";
                    string TemplateFile =string.Empty;
                    int rangeDefualtLength = 0;
                    switch (model.GetExcelFormType)
                    {
                        case ExcelForm.EmptyForm1:
                            TemplateFile=  HttpContext.Current.Server.MapPath(folderPath + "Apply.xlsx");
                            rangeDefualtLength = 17;
                        break;
                        default:
                            throw new Exception("can't get method with excel base file.");
                    }

                    XLWorkbook exl = new XLWorkbook(TemplateFile);
                    IXLWorksheet wsht = exl.Worksheet(1);
                    wsht.RangeUsed().SetAutoFilter();

                    //var defualtRowHeight = wsht.Row(2).Height;
                    var defualtRowHeight = 80;
                    var sexCell= wsht.Cell("G2").Value;
                    int satartRow = 3;
                    /*
                    報名編號	隊名	參賽組別	教練	聯絡人	聯絡電話	E-mail	其他	隊長	隊員
                    */
                   string docTitle =  "比賽報名明細表";
                    wsht.Cell(1,1).Value =string.Concat(model.ActivityName ,docTitle);
                    wsht.Cell(2,2).Value =model.ActivityName ;
                    if(model.GetExcelFormType == ExcelForm.EmptyForm1)
                    {
                       foreach (var data in model.ApplyListData)
                        {
                                wsht.Cell(++satartRow,1).Value = data.ApplyNumber;
                                wsht.Cell(satartRow,2).Value = data.ApplyTeamName;
                                wsht.Cell(satartRow,3).Value = model.GroupList.Where(o=>o.Key==data.GroupID).First().Value;
                                wsht.Cell(satartRow,4).Value = data.Coach;
                                wsht.Cell(satartRow,5).Value = data.Contact;
                                wsht.Cell(satartRow,6).Value = data.ContactPhone;
                                wsht.Cell(satartRow,7).Value = data.ContactEmail;
                                wsht.Cell(satartRow,8).Value = data.Remark;
 
                                int mbIndex = 10;
                                foreach (var mb in data.MemberInfo)
                                {
                                    string memberInfo =string.Format("{0}\r\n{1}\r\n{2}",mb.MemberName,mb.MemberBirthday,mb.MemberIdentityID);
                                    if(mb.MemberType=="Leader")
                                    wsht.Cell(satartRow,9).Value = memberInfo;
                                    else
                                    {
                                    wsht.Cell(satartRow,mbIndex).Value = memberInfo;
                                    mbIndex++;
                                    }
                                 }
                                wsht.Row(satartRow).Height = defualtRowHeight;
                           
                        }
                    }

                        wsht.RangeUsed().SetAutoFilter(); //Add autofilter
                        wsht.Columns().AdjustToContents();
                        //wsht.FirstRowUsed().Style.Fill.BackgroundColor = XLColor.PowderBlue;
                        //wsht.FirstRowUsed().Style.Font.Bold = true;
                        satartRow = wsht.RowsUsed().Count();

                        IXLRange rangeFirst = wsht.Range(1, 1, 1, rangeDefualtLength);
                        rangeFirst.Style.Fill.BackgroundColor = XLColor.PowderBlue;
                        rangeFirst.Style.Font.Bold = true;
                        rangeFirst.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        IXLRange range = wsht.Range(4, 1, satartRow, rangeDefualtLength);
                        range.Style.Font.FontSize = 12;
                          //水平置中
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                         //垂直置中
                         range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; ;

                        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        range.Style.Alignment.WrapText = true;
                        range.Style.Font.SetFontName("細明體");



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


    }
}