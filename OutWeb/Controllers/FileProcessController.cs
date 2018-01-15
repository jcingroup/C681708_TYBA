using OutWeb.Entities;
using OutWeb.Models.Manage.ExportExcelModels;
using OutWeb.Models.Manage.ExportExcelModels.QuestionnaireStatisticsReplyModels;
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
        public ActionResult GetFile(string type, int? ID)
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
                    case "QuestionnairesReply": //問卷統計 回覆清單
                        chkHasData = CheckQuestionnairesHasData((int)ID);
                        if (!chkHasData)
                            break;
                        sm = GetReplyWithQuestionnaires((int)ID);
                        break;

                    case "TrainSignEmptyList": //研討會 空白簽到單
                        //chkHasData = CheckTrainHasData((int)ID);
                        if (!chkHasData)
                            break;
                        sm = GetRegistrationCountWithTrain((int)ID,ExcelForm.EmptyForm1);
                        break;
                    case "TrainEmptyList": //研討會 匯出清單
                        //chkHasData = CheckTrainHasData((int)ID);
                        if (!chkHasData)
                            break;
                        sm = GetRegistrationCountWithTrain((int)ID, ExcelForm.EmptyForm2);
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
                msg = "尚未有回覆資料，無法產生清單";
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

        #region 問卷統計

        private bool CheckQuestionnairesHasData(int ID)
        {
            bool isHasData = true;
            using (var db = new DBEnergy())
            {
                isHasData = db.問卷答案主檔.Where(o => o.對應問卷主檔索引 == ID).Count() > 0;
            }
            return isHasData;
        }

        private MemoryStream GetReplyWithQuestionnaires(int? ID)
        {
            MemoryStream sm;
            using (var qModule = new QuestionnaireStatisticsModule())
            {
                var model = qModule.GetReplyDataByID((int)ID);
                sm = FileRepo.ObjectToExcel<ReplyDataModel>(model);
            }
            return sm;
        }

        #endregion 問卷統計

        #region 研討會

        private bool CheckTrainHasData(int ID)
        {
            bool isHasData = true;
            using (var db = new DBEnergy())
            {
                isHasData = db.研討會報名對應檔.Where(o => o.對應研討會主索引 == ID).Count() > 0;
            }

            return isHasData;
        }

        private MemoryStream GetRegistrationCountWithTrain(int? ID, ExcelForm formType)
        {
            TrainApplyModule qModule = new TrainApplyModule();
            OutWeb.Models.Manage.ExportExcelModels.TrainSignListModels.ReplyDataModel model = qModule.GetTrainApplyParticipantsDataByTrainID((int)ID);
            model.GetExcelFormType = formType;
            MemoryStream sm = FileRepo.ObjectToExcel(model);
            return sm;
        }

        #endregion 研討會
    }
}