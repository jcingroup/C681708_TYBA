using System.ComponentModel;

namespace OutWeb.Enums
{
    /// <summary>
    /// 列表狀態列舉
    /// </summary>
    public enum StatusEnums
    {
        /// <summary>
        /// 文章狀態列舉
        /// </summary>
        [Description("未設定")]
        NotSet,

        /// <summary>
        /// 已發布
        /// </summary>
        [Description("已發佈")]
        Y,

        /// <summary>
        /// 草稿
        /// </summary>
        [Description("草稿")]
        N,
    }

    public enum DisplayEnums
    {
        /// <summary>
        /// 學校分類顯示或停用列舉
        /// </summary>
        [Description("未設定")]
        NotSet,

        /// <summary>
        /// 啟用
        /// </summary>
        [Description("啟用")]
        Y,

        /// <summary>
        /// 停用
        /// </summary>
        [Description("停用")]
        N,
    }

    /// <summary>
    /// 使用者角色列舉
    /// </summary>
    public enum UserRoleEnum
    {
        [Description("未設定")]
        NotSet,

        [Description("使用者")]
        ADMIN,

        [Description("超級管理者")]
        SUPERADMIN,

        [Description("管理者")]
        USER,
    }

    /// <summary>
    /// 列表查詢方法列舉
    /// </summary>
    public enum ListMethodType
    {
        [Description("未設定")]
        NotSet,

        [Description("最新消息")]
        NEWS,

        [Description("案例分享")]
        WORKS,

        [Description("線上課程")]
        COURSE,

        [Description("分類管理")]
        TYPEMANAGE,

        [Description("問卷調查")]
        QUESTIONNAIRES,

        [Description("研討會")]
        TRAIN,
        [Description("研討會報名")]
        TRAINAPPLY,
        [Description("三竹簡訊")]
        SMS,
        [Description("HiNetFax傳真")]
        FAX,
        [Description("EMAIL")]
        EMAIL,
        [Description("出本品分類")]
        BOOKKIND,
        [Description("外部連結")]
        LINK,

    }

    /// <summary>
    /// 心得分想分類列舉
    /// </summary>
    public enum FeedbackType
    {
        [Description("未設定")]
        NotSet,

        /// <summary>
        /// 部落客推薦
        /// </summary>
        [Description("部落客推薦")]
        B,

        /// <summary>
        /// 學員心得分享
        /// </summary>
        [Description("學員心得分享")]
        S,

        /// <summary>
        /// 小編碎碎念
        /// </summary>
        [Description("小編碎碎念")]
        C,
    }

    /// <summary>
    /// 分頁每頁數量列舉
    /// </summary>
    public enum PageSizeConfig
    {
        /// <summary>
        /// 列表資料預設筆數3
        /// </summary>
        SIZE03 = 3,

        /// <summary>
        /// 列表資料預設筆數5
        /// </summary>
        SIZE05 = 5,

        /// <summary>
        /// 列表資料預設筆數6
        /// </summary>
        SIZE06 = 6,

        /// <summary>
        /// 列表資料預設筆數10
        /// </summary>
        SIZE10 = 10,
        /// <summary>
        /// 列表資料預設筆數30
        /// </summary>
        SIZE30 = 30,
    }

    /// <summary>
    /// 代理語言學校分類
    /// </summary>
    public enum SchoolType
    {
        [Description("菲律賓")]
        Type1,
    }

    /// <summary>
    /// 語系列舉
    /// </summary>
    public enum Language
    {
        [Description("")]
        NotSet,

        /// <summary>
        /// English (United States)
        /// </summary>
        [Description("en-US")]
        enUS,

        /// <summary>
        /// Taiwan
        /// </summary>
        [Description("zh-TW")]
        zhTW,

        /// <summary>
        /// Chinese (PRC)
        /// </summary>
        [Description("zh-CN")]
        zhCN
    }

    public enum ServiceStdEnums
    {
        [Description("")]
        NotSet,

        /// <summary>
        /// 企業合作專區
        /// </summary>
        [Description("企業合作專區")]
        SERVICE1 = 1,

        /// <summary>
        /// 語言學校代辦
        /// </summary>
        [Description("語言學校代辦")]
        SERVICE2 = 2,

        /// <summary>
        /// 職涯能力規畫講座
        /// </summary>
        [Description("職涯能力規畫講座")]
        SERVICE3 = 3,

        /// <summary>
        /// 團體客製化遊學課程
        /// </summary>
        [Description("團體客製化遊學課程")]
        SERVICE4 = 4,

        /// <summary>
        /// 留學諮詢服務
        /// </summary>
        [Description("留學諮詢服務")]
        SERVICE5 = 5,

        /// <summary>
        /// 關於智德
        /// </summary>
        [Description("關於智德")]
        SERVICE6 = 6,
    }

    public enum SchoolStdEnums
    {
        [Description("")]
        NotSet,

        /// <summary>
        /// 美國社區大學
        /// </summary>
        [Description("美國社區大學")]
        SCHOOL5 = 5,

        /// <summary>
        /// 澳洲升學直通車計畫
        /// </summary>
        [Description("澳洲升學直通車計畫")]
        SCHOOL6 = 6,

        /// <summary>
        /// 全球實習課程
        /// </summary>
        [Description("全球實習課程")]
        SCHOOL4 = 4,

        /// <summary>
        /// 語言學習趨勢
        /// </summary>
        [Description("語言學習趨勢")]
        SCHOOL1 = 1,

        /// <summary>
        /// 前進菲律賓
        /// </summary>
        [Description("前進菲律賓")]
        SCHOOL3 = 3,

        /// <summary>
        /// 全方位聽說讀寫
        /// </summary>
        [Description("全方位聽說讀寫")]
        SCHOOL2 = 2,
    }

    public enum KnowledgeStdEnums
    {
        [Description("")]
        NotSet,

        /// <summary>
        /// 空服員專業英語培訓
        /// </summary>
        [Description("空服員專業英語培訓")]
        KNOWLEDGE3 = 3,

        /// <summary>
        /// 企業人士英語課程
        /// </summary>
        [Description("企業人士英語課程")]
        KNOWLEDGE4 = 4,

        /// <summary>
        /// 專業商用英語
        /// </summary>
        [Description("專業商用英語")]
        KNOWLEDGE1 = 1,

        /// <summary>
        /// 專業醫療英語
        /// </summary>
        [Description("專業醫療英語")]
        KNOWLEDGE2 = 2,

        /// <summary>
        /// 英語教學證照課程
        /// </summary>
        [Description("英語教學證照課程")]
        KNOWLEDGE5 = 5,
    }
}