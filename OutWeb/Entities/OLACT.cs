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
    
    public partial class OLACT
    {
        public int ID { get; set; }
        public string ACTITLE { get; set; }
        public System.DateTime BUD_DT { get; set; }
        public int BUD_ID { get; set; }
        public System.DateTime UPD_DT { get; set; }
        public int UPD_ID { get; set; }
        public Nullable<double> SQ { get; set; }
        public string PUB_DT_STR { get; set; }
        public bool DISABLE { get; set; }
        public string ACT_DATE_DESC { get; set; }
        public string APPLY_DATE_BEGIN { get; set; }
        public string APPLY_DATE_END { get; set; }
        public string ACT_NUM { get; set; }
        public string ACT_CONTENT { get; set; }
    }
}
