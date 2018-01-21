using OutWeb.Models.Manage;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Modules.Manage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace OutWeb.Repositories
{
    public class FileRepository
    {
        private string savePath { get; set; }
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        public FileRepository()
        {
        }

        public FileRepository(string savePath)
        {
            this.savePath = savePath;
        }

        #region 儲存處理函式

        /// <summary>
        ///
        /// </summary>
        /// <param name="vm"></param>
        public void SaveFileToDB(FilesModel vm)
        {
            FileModule fileModule = new FileModule();
            fileModule.SaveFiles(vm);
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="myFile"></param>
        public void UploadFile(string uploadType, FilesModel vm, List<HttpPostedFileBase> files, string mode)
        {
            string serverMapPath = string.Empty;

            if (uploadType == "upload")
                serverMapPath = "/Content/Upload/Manage/Files/Temp/";
            else
                serverMapPath = "/Content/Upload/Manage/Files/";

            foreach (var m in vm.MemberDataMultiple)
                m.FilePath = HttpContext.Current.Server.MapPath(serverMapPath + m.FileName).Replace(rootPath, "");

            if (files != null && files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string strFileName = uploadType == "upload" ? files[i].FileName : Guid.NewGuid().ToString() + Path.GetExtension(files[i].FileName);
                    string strFilePath = HttpContext.Current.Server.MapPath(serverMapPath + strFileName);
                    string realFileName = files[i].FileName;
                    files[i].SaveAs(strFilePath);

                    #region data binding to model

                    vm.MemberDataMultiple.Add(new FileViewModel()
                    {
                        RealFileName = realFileName,
                        FilePath = strFilePath.Replace(rootPath, ""),
                        FileName = strFileName,
                        FileUrl = serverMapPath.Substring(1, serverMapPath.Length - 1) + strFileName,
                    });

                    #endregion data binding to model
                }
            }
        }

        #endregion 儲存處理函式

        /// <summary>
        /// 用於HINETFAX所需的TIL文件
        /// </summary>
        /// <param name="filename">原始檔案名稱</param>
        /// <param name="strOutput">轉檔後TIL內容</param>
        /// <param name="encoding">編碼模式</param>
        public FileViewModel SaveTILFile(String filename, String strOutput, int encoding)
        {
            string serverMapPath = string.Empty;
            serverMapPath = "~/Content/Upload/Manage/Files/";
            string strRealFileName = filename;
            string strFileName = Guid.NewGuid().ToString() + Path.GetExtension(filename);
            string strFilePath = HttpContext.Current.Server.MapPath(serverMapPath + strFileName).Replace(rootPath, "");

            string FileUrl = serverMapPath.Substring(2, serverMapPath.Length - 2) + strFileName;

            var mv = new FileViewModel();

            mv.RealFileName = strRealFileName;
            mv.FilePath = strFilePath;
            mv.FileName = strFileName;
            mv.FileUrl = FileUrl;

            Encoding ecp = Encoding.GetEncoding(950);
            File.WriteAllText(string.Concat(rootPath, strFilePath), strOutput, ecp);

            return mv;
        }

        /// <summary>
        /// 將傳真所需的檔案格式另存成既定格式
        /// </summary>
        /// <param name="filePath">原始檔案路徑</param>
        /// <param name="oriName">原始檔案內容</param>
        /// <param name="hiNetUser">HINET帳戶名稱</param>
        /// <param name="timeStr">時間字串(6碼)</param>
        /// <param name="dateStr">年份日期字串</param>
        public Param CopyFile(String filePath, String oriName, String hiNetUser, String timeStr, String dateStr)
        {
            Param li = new Param();
            string serverMapPath = string.Empty;

            serverMapPath = "~/Content/Upload/Manage/Files/Fax/" + dateStr + "/"; //創建日期資料夾

            string tilNeedStr = "HN" + hiNetUser + "." + timeStr; //檔名格式 HNNo.序號.tif (序號自訂，最多6碼) e.g.HN90000376.911122.til

            string file_ext = Path.GetExtension(oriName); //取得檔案副檔名
            //檔案類型
            if (file_ext.ToLower().Contains(".docx"))
            {
                tilNeedStr += ".docx";
            }
            else if (file_ext.ToLower().Contains(".doc"))
            {
                tilNeedStr += ".doc";
            }
            else if (file_ext.ToLower().Contains(".pdf"))
            {
                tilNeedStr += ".pdf";
            }
            else
            {
                tilNeedStr += ".til";
            }

            string strFilePath = HttpContext.Current.Server.MapPath(serverMapPath); //檢查資料夾目錄是否存在

            if (!Directory.Exists(strFilePath))
            {
                //新增資料夾
                Directory.CreateDirectory(strFilePath);
            }

            strFilePath = HttpContext.Current.Server.MapPath(serverMapPath + tilNeedStr);  //產生CopyTo需要的路徑

            var relatePath = strFilePath.Replace(rootPath, ""); //取相對路徑
            try
            {
                FileInfo fi = new FileInfo(@filePath);
                fi.CopyTo(@strFilePath, true); // existing file will be overwritten
                li.success = true;
                li.fileName = tilNeedStr;
                li.filePath = relatePath;
                li.oriName = oriName;
            }
            catch (Exception ex)
            {
                li.success = false;
                li.errorMsg = "[傳真維護啟動發送] 複製檔案名稱為: " + oriName + " 時發生錯誤，錯誤訊息:" + ex.ToString();
            }
            return li;
        }

        public class Param
        {
            public bool success { get; set; }
            public string errorMsg { get; set; }
            public string fileName { get; set; }
            public string filePath { get; set; }
            public string oriName { get; set; }
        }
    }
}