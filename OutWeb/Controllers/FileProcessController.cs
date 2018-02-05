using OutWeb.Entities;
using OutWeb.Models.Manage.ApplyMaintainModels;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    public class FileProcessController : Controller
    {
        private FileRepository m_fileRepo = new FileRepository();

        private FileRepository FileRepo
        {
            get { return m_fileRepo; }
        }

        private static readonly char[] InvalidFilenameChars = Path.GetInvalidFileNameChars();

        [HttpGet]
        public ActionResult Download(string fileGuid, string fileName)
        {
            if (fileName.IndexOfAny(InvalidFilenameChars) >= 0)
                return new EmptyResult();

            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.ms-excel", fileName);
            }
            else
                return new EmptyResult();
        }

        [HttpPost]
        public ActionResult GetFile(string type, int? ID, int? groupID)
        {
            bool isSuccess = true;
            bool chkHasData = true;
            string msg = string.Empty;
            string handle = Guid.NewGuid().ToString();
            MemoryStream sm = null;
            try
            {
                switch (type)
                {
                    case "ApplyReply": //問卷統計 回覆清單
                        chkHasData = CheckActivityHasData((int)ID);
                        if (!chkHasData)
                            break;
                        sm = GetReplyWithApply((int)ID, (int)groupID);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                msg = "系統發生未知錯誤" + ex.Message;
                return new JsonResult() { Data = new { success = isSuccess, msg = msg } };
            }

            if (!chkHasData)
            {
                isSuccess = false;
                msg = "無法取得活動資料，無法產生清單";
                return Json(new { success = isSuccess, msg = msg });
            }
            TempData[handle] = sm.ToArray();
            return new JsonResult()
            {
                Data = new { success = isSuccess, msg = msg, FileGuid = handle, FileName = GetCreatetime() + ".xlsx" }
            };
        }

        #region Public Function

        /// <summary>
        /// 時間戳記
        /// </summary>
        /// <returns></returns>
        private string GetCreatetime()
        {
            DateTime DateStart = new DateTime(1970, 1, 1, 8, 0, 0);
            return Convert.ToInt32((DateTime.UtcNow.AddHours(8) - DateStart).TotalSeconds).ToString();
        }

        #endregion Public Function

        #region 報名

        private bool CheckActivityHasData(int ID)
        {
            bool isHasData = true;
            using (var db = new TYBADB())
            {
                isHasData = db.OLACT.Where(o => o.ID == ID).Count() > 0;
            }
            return isHasData;
        }

        private MemoryStream GetReplyWithApply(int ID, int groupID)
        {
            MemoryStream sm;
            ApplyExcelReplyDataModel model = new ApplyExcelReplyDataModel();
            using (var module = new ApplyMaintainModule())
            {
                model.ActivityID = ID;
                var result = module.GetApplyDetailsForExcel(model);
                if (groupID > 0)
                {
                    result.ApplyListData = result.ApplyListData.Where(o => o.GroupID == groupID).ToList();
                }
                model.GetExcelFormType = Models.Manage.ExcelForm.EmptyForm1;
                sm = FileRepo.ObjectToExcel<ApplyExcelReplyDataModel>(model);
            }
            return sm;
        }

        #endregion 報名
    }
}