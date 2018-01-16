using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage
{
    /// <summary>
    /// 檔案基礎模型
    /// </summary>
    public class FileViewModel
    {
        public int? ID { get; set; }
        public string RealFileName { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public string FileUrl { get; set; }

        public int Identify { get; set; }
    }
}