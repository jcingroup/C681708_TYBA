using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ExportExcelModels
{
    public class ReplyBase
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public ExcelForm GetExcelFormType { get; set; }
    }
    public enum ExcelForm
    {
        EmptyForm1 = 1,
        EmptyForm2 = 2
    }
}