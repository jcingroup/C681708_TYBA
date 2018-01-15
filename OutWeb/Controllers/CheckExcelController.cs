using ClosedXML.Excel;
using OutWeb.ActionFilter;
using OutWeb.Authorize;
using OutWeb.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;


namespace OutWeb.Controllers
{
    public class CheckExcelController : Controller
    {

        [HttpPost]
        public JsonResult CheckSMSExcel(HttpPostedFileBase file)
        {
            JsonParam jp = new JsonParam();
            jp.validate = true;


            try
            {
                string FN = file.FileName;

                #region 驗證
                string[] allowExt = new string[] { ".XLSX" };
                string file_ext = Path.GetExtension(FN); //取得副檔名
                if (!allowExt.Contains(file_ext.ToUpper()))
                {
                    jp.validate = false;
                    jp.errorMsg = "檔案格式錯誤!";
                    return Json(jp, JsonRequestBehavior.DenyGet);
                }

                int byteCount = file.ContentLength;
                int limitSize = 2 * 1024 * 1024;

                if (byteCount > limitSize)
                {
                    jp.validate = false;
                    jp.errorMsg = "檔案大小超過限制!";
                    return Json(jp, JsonRequestBehavior.DenyGet);
                }
                #endregion

                jp = CheckSMSContentValidate(file);

                if (jp.validate == false)
                {
                    return Json(jp, JsonRequestBehavior.DenyGet);
                }

                return Json(jp, JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                jp.validate = false;
                jp.errorMsg = ex.ToString();
                return Json(jp, JsonRequestBehavior.DenyGet);
            }
        }

        #region SMS Excel內容驗證
        private JsonParam CheckSMSContentValidate(HttpPostedFileBase file)
        {
            JsonParam jp = new JsonParam();
            XLWorkbook excelWB = new XLWorkbook(file.InputStream);
            IXLWorksheet sheet = excelWB.Worksheet(1);
            int RowCount = sheet.RowsUsed().Count();
            jp.count = 0;

            jp.validate = true;
            if (RowCount > 1)
            {
                for (int i = 2; i <= RowCount; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 1).Value)))
                    {
                        var ckValue = sheet.Cell(i, 1).Value;

                        if (!IsNumber(ckValue.ToString()))
                        {
                            jp.validate = false;
                            jp.errorMsg = "【A" + i + "】 手機號碼只能輸入數字，例:0912345678";
                            return jp;
                        }

                        if (ckValue.ToString().Substring(0, 2) != "09")
                        {
                            jp.validate = false;
                            jp.errorMsg = "【A" + i + "】 手機號碼格式錯誤，開頭需09";
                            return jp;
                        }
                        jp.count++;
                    }

                    if (string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 1).Value)) && !string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 2).Value)))
                    {
                        jp.validate = false;
                        jp.errorMsg = "【A" + i + "】 未輸入手機號碼";
                        return jp;
                    }

                }
            }
            else
            {
                jp.validate = false;
                jp.errorMsg = "檔案無內容";
                return jp;
            }

            return jp;


        }
        #endregion

        //檢查是否為數字
        private bool IsNumber(string inputStr)
        {
            long n = 0;
            if (long.TryParse(inputStr, out n))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpPost]
        public JsonResult CheckFAXExcel(HttpPostedFileBase file)
        {
            JsonParam jp = new JsonParam();
            jp.validate = true;

            try
            {
                string FN = file.FileName;

                #region 驗證
                string[] allowExt = new string[] { ".XLSX" };
                string file_ext = Path.GetExtension(FN); //取得副檔名
                if (!allowExt.Contains(file_ext.ToUpper()))
                {
                    jp.validate = false;
                    jp.errorMsg = "檔案格式錯誤!";
                    return Json(jp, JsonRequestBehavior.DenyGet);
                }

                int byteCount = file.ContentLength;
                int limitSize = 2 * 1024 * 1024;

                if (byteCount > limitSize)
                {
                    jp.validate = false;
                    jp.errorMsg = "檔案大小超過限制!";
                    return Json(jp, JsonRequestBehavior.DenyGet);
                }
                #endregion

                jp = CheckFAXContentValidate(file);

                if (jp.validate == false)
                {
                    return Json(jp, JsonRequestBehavior.DenyGet);
                }

                return Json(jp, JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                jp.validate = false;
                jp.errorMsg = ex.ToString();
                return Json(jp, JsonRequestBehavior.DenyGet);
            }


        }


        #region FAX Excel內容驗證
        private JsonParam CheckFAXContentValidate(HttpPostedFileBase file)
        {
            JsonParam jp = new JsonParam();
            XLWorkbook excelWB = new XLWorkbook(file.InputStream);
            IXLWorksheet sheet = excelWB.Worksheet(1);
            int RowCount = sheet.RowsUsed().Count();

            jp.validate = true;
            if (RowCount > 1)
            {
                for (int i = 2; i <= RowCount; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 1).Value)))
                    {

                        var ckValue = sheet.Cell(i, 1).Value;

                        if (!IsNumber(ckValue.ToString()))
                        {
                            jp.validate = false;
                            jp.errorMsg = "【A" + i + "】 傳真號碼只能輸入數字，例:8862345678";
                            return jp;
                        }

                        if (ckValue.ToString().Substring(0, 3) != "886")
                        {
                            jp.validate = false;
                            jp.errorMsg = "【A" + i + "】 傳真號碼格式錯誤";
                            return jp;
                        }

                    }
                    else
                    {
                        jp.validate = false;
                        jp.errorMsg = "【B" + i + "】 請輸入傳真號碼";
                        return jp;
                    }

                    if (string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 2).Value)))
                    {
                        jp.validate = false;
                        jp.errorMsg = "【B" + i + "】 請輸入收件者公司";
                        return jp;
                    }

                    if (string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 3).Value)))
                    {
                        jp.validate = false;
                        jp.errorMsg = "【C" + i + "】 請輸入收件者姓名";
                        return jp;
                    }

                    jp.count++;
                }
            }
            else
            {
                jp.validate = false;
                jp.errorMsg = "檔案無內容";
                return jp;
            }

            return jp;


        }
        #endregion



        [HttpPost]
        public JsonResult CheckEMAILExcel(HttpPostedFileBase file)
        {
            JsonParam jp = new JsonParam();
            jp.validate = true;

            try
            {
                string FN = file.FileName;

                #region 驗證

                string[] allowExt = new string[] { ".XLSX" };
                string file_ext = Path.GetExtension(FN); //取得副檔名
                if (!allowExt.Contains(file_ext.ToUpper()))
                {
                    jp.validate = false;
                    jp.errorMsg = "檔案格式錯誤!";
                    return Json(jp, JsonRequestBehavior.DenyGet);
                }

                int byteCount = file.ContentLength;
                int limitSize = 2 * 1024 * 1024;

                if (byteCount > limitSize)
                {
                    jp.validate = false;
                    jp.errorMsg = "檔案大小超過限制!";
                    return Json(jp, JsonRequestBehavior.DenyGet);
                }

                #endregion 驗證

                jp = CheckEamilContentValidate(file);

                if (jp.validate == false)
                {
                    return Json(jp, JsonRequestBehavior.DenyGet);
                }

                return Json(jp, JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                jp.validate = false;
                jp.errorMsg = ex.ToString();
                return Json(jp, JsonRequestBehavior.DenyGet);
            }
        }

        #region EMAL Excel內容驗證

        private JsonParam CheckEamilContentValidate(HttpPostedFileBase file)
        {
            JsonParam jp = new JsonParam();
            XLWorkbook excelWB = new XLWorkbook(file.InputStream);
            IXLWorksheet sheet = excelWB.Worksheet(1);
            int RowCount = sheet.RowsUsed().Count();

            jp.validate = true;
            if (RowCount > 1)
            {
                for (int i = 2; i <= RowCount; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 1).Value)))
                    {
                        var ckValue = sheet.Cell(i, 1).Value;

                        try
                        {
                            MailAddress m = new MailAddress((ckValue.ToString()));
                        }
                        catch (FormatException)
                        {
                            jp.validate = false;
                            jp.errorMsg = "【A" + i + "】 EMAIL格式錯誤";
                            return jp;
                        }
                    }
                    else
                    {
                        jp.validate = false;
                        jp.errorMsg = "【A" + i + "】 請輸入EAMIL位址";
                        return jp;
                    }

                    //if (string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 2).Value)))
                    //{
                    //    jp.validate = false;
                    //    jp.errorMsg = "【B" + i + "】 請輸入收件者姓名";
                    //    return jp;
                    //}

                    //if (string.IsNullOrEmpty(Convert.ToString(sheet.Cell(i, 3).Value)))
                    //{
                    //    jp.validate = false;
                    //    jp.errorMsg = "【C" + i + "】 請輸入收件者公司";
                    //    return jp;
                    //}

                    jp.count++;
                }
            }
            else
            {
                jp.validate = false;
                jp.errorMsg = "檔案無內容";
                return jp;
            }

            return jp;
        }


        #endregion EMAL Excel內容驗證

        public class JsonParam
        {
            public bool validate { get; set; }
            public string errorMsg { get; set; }
            public int count { get; set; }
        }

    }
}