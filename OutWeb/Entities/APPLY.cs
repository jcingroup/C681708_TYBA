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
    
    public partial class APPLY
    {
        public int ID { get; set; }
        public int MAP_ACT_ID { get; set; }
        public string TEAM_NM { get; set; }
        public int MAP_ACT_GUP_ID { get; set; }
        public string CONTACT { get; set; }
        public string CONTACT_PHONE { get; set; }
        public string REMRK { get; set; }
        public string APPLY_IDEN_NUM { get; set; }
        public string TEAM_COACH { get; set; }
        public string EMAIL { get; set; }
        public System.DateTime BUD_DT { get; set; }
        public bool APPLY_SUCCESS { get; set; }
        public System.DateTime UPD_DT { get; set; }
    }
}
