//using OutWeb.Entities;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace OutWeb.Service
//{
//    public class ExcelProcessService
//    {
//        private DBEnergy m_DB = new DBEnergy();

//        private DBEnergy DB
//        { get { return this.m_DB; } set { this.m_DB = value; } }

//        public MemoryStream ExportExcel()
//        {
//            //取出要匯出Excel的資料
//            List<問卷題目類型檔> rangerList = DB.問卷題目類型檔.ToList();

//            //建立Excel
//            ExcelPackage ep = new ExcelPackage();

//            //建立第一個Sheet，後方為定義Sheet的名稱
//            ExcelWorksheet sheet = ep.Workbook.Worksheets.Add("FirstSheet");
//            //欄:直，因為要從第1欄開始，所以初始為1
//            int col = 1;

//            //標題列
//            sheet.Cells[1, col++].Value = "ID";
//            sheet.Cells[1, col++].Value = "Name";

//            //資料列:橫
//            int row = 2;
//            foreach (var item in rangerList)
//            {
//                col = 1;//每換一列，欄位要從1開始 指定Sheet的欄與列(欄名列號ex.A1,B20，在這邊都是用數字)，將資料寫入
//                sheet.Cells[row, col++].Value = item.主索引;
//                sheet.Cells[row, col++].Value = item.問卷類型名稱;
//                row++;
//            }

//            //資料流寫入
//            MemoryStream fileStream = new MemoryStream();
//            ep.SaveAs(fileStream);
//            ep.Dispose();
//            //不重新將位置設為0，excel開啟後會出現錯誤
//            fileStream.Position = 0;
//            return fileStream;
//            //return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExportRanger.xlsx");
//        }

//        public void ReadExcel()
//        {
//            //開檔
//            using (FileStream fs = new FileStream(@"C:\ExportRanger.xlsx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
//            {
//                //載入Excel檔案
//                using (ExcelPackage ep = new ExcelPackage(fs))
//                {
//                    ExcelWorksheet sheet = ep.Workbook.Worksheets[1];//取得Sheet1
//                    int startRowNumber = sheet.Dimension.Start.Row;//起始列編號，從1算起
//                    int endRowNumber = sheet.Dimension.End.Row;//結束列編號，從1算起
//                    int startColumn = sheet.Dimension.Start.Column;//開始欄編號，從1算起
//                    int endColumn = sheet.Dimension.End.Column;//結束欄編號，從1算起

//                    bool isHeader = true;
//                    if (isHeader)//有包含標題
//                    {
//                        startRowNumber += 1;
//                    }

//                    ////寫入標題文字
//                    var c1 = sheet.Cells[1, 1].Value;
//                    var c2 = sheet.Cells[1, 2].Value;
//                    for (int currentRow = startRowNumber; currentRow <= endRowNumber; currentRow++)
//                    {
//                        ExcelRange range = sheet.Cells[currentRow, startColumn, currentRow, endColumn];//抓出目前的Excel列
//                        if (range.Any(c => !string.IsNullOrEmpty(c.Text)) == false)//這是一個完全空白列(使用者用Delete鍵刪除動作)
//                        {
//                            continue;//略過此列
//                        }
//                        //讀值
//                        string cellValue = sheet.Cells[currentRow, 1].Text;//讀取格式化過後的文字(讀取使用者看到的文字)

//                        //寫值
//                        //sheet.Cells[currentRow, 1].Value = cellValue + "test";
//                    }

//                    //建立檔案
//                    //using (FileStream createStream = new FileStream(@"D:\output.xlsx", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
//                    //{
//                    //    ep.SaveAs(createStream);//存檔
//                    //}//end using
//                }//end   using
//            }//end using
//        }
//    }
//}