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
    
    public partial class 問卷答案明細檔
    {
        public int 主索引 { get; set; }
        public int 對應問卷答案索引 { get; set; }
        public int 對應問卷題目索引 { get; set; }
        public int 對應問題類型索引 { get; set; }
        public Nullable<int> 問卷答案選項值 { get; set; }
        public string 問卷答案文字值 { get; set; }
        public System.DateTime 回答日期 { get; set; }
    }
}
