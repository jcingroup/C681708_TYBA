//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace OutWeb.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class 新聞
    {
        public int 主索引 { get; set; }
        public int 分類代碼 { get; set; }
        public System.DateTime 發稿時間 { get; set; }
        public string 發稿人 { get; set; }
        public string 標題 { get; set; }
        public string 內容 { get; set; }
        public int 登錄人 { get; set; }
        public System.DateTime 修改日期 { get; set; }
        public int 修改人 { get; set; }
        public string 連結位址 { get; set; }
        public double 排序 { get; set; }
        public bool 顯示狀態 { get; set; }
        public bool 首頁顯示 { get; set; }
        public Nullable<int> 流水號 { get; set; }
        public int 樣板代碼 { get; set; }
    }
}
