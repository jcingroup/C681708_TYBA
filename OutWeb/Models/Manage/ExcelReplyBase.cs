using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage
{
    public class ExcelReplyBase
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public ExcelForm GetExcelFormType { get; set; }
    }
    public enum ExcelForm
    {
        EmptyForm1 = 1,
        EmptyForm2 = 2,
        EmptyForm3 = 3,//匯出手機
        EmptyForm4 = 4,//匯出傳真
        EmptyForm5 = 5,//匯出Email
    }
}