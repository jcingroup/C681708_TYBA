using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models.Manage;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Provider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace OutWeb.Modules.Manage
{
    public class FileModule : IDisposable
    {
        private TYBADB DB = new TYBADB();
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        public void SaveFiles(FilesModel model)
        {
            if (model.ID > 0)
            {
                List<FILEBASE> filterRemove = new List<FILEBASE>();
                if (model.UploadIdentify == FileUploadIdentifyType.NOTSET)
                {
                    filterRemove = this.DB.FILEBASE
                                        .Where(o => !model.OldFileIds.Contains(o.ID) &&
                                        o.MAP_ID == model.ID &&
                                        o.MAP_SITE == model.ActionName &&
                                        o.FILE_TP == "F"
                                        ).ToList();
                }
                else
                {
                    filterRemove = this.DB.FILEBASE
                   .Where(o => !model.OldFileIds.Contains(o.ID) &&
                   o.MAP_ID == model.ID &&
                   o.MAP_SITE == model.ActionName &&
                   o.FILE_TP == "F" &&
                   o.IDENTIFY_KEY == (int)model.UploadIdentify
                   ).ToList();
                }

                if (filterRemove.Count > 0)
                {
                    foreach (var f in filterRemove)
                        File.Delete(string.Concat(rootPath, f.FILE_PATH));
                    //刪除舊檔
                    this.DB.FILEBASE.RemoveRange(filterRemove);
                    this.DB.SaveChanges();
                }
            }

            //存檔
            foreach (var f in model.MemberDataMultiple)
            {
                int sq = model.MemberDataMultiple.IndexOf(f);
                FILEBASE file = new FILEBASE()
                {
                    FILE_RANDOM_NM = f.FileName,
                    MAP_SITE = model.ActionName,
                    SQ = sq,
                    MAP_ID = model.ID,
                    FILE_REL_NM = f.RealFileName,
                    BUD_DT = DateTime.UtcNow.AddHours(8),
                    BUD_ID = UserProvider.Instance.User.ID,
                    FILE_TP = "F",
                    FILE_PATH = f.FilePath,
                    URL_PATH = f.FileUrl,
                    IDENTIFY_KEY = model.UploadIdentify == FileUploadIdentifyType.NOTSET ? default(int?) : (int)model.UploadIdentify,
                };
                this.DB.FILEBASE.Add(file);
                try
                {
                    this.DB.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw;
                }
                f.ID = file.ID;
            }
        }

        public List<FileViewModel> GetFiles(int ID, string actionName, string fileType, FileUploadIdentifyType fileIdentity = FileUploadIdentifyType.NOTSET)
        {
            List<FileViewModel> fileList = new List<FileViewModel>();
            fileList = this.DB.FILEBASE
                    .Where(o => o.MAP_ID == ID
                    && o.MAP_SITE.StartsWith(actionName)
                    && o.FILE_TP == fileType
                    && (fileIdentity == FileUploadIdentifyType.NOTSET ? true : o.IDENTIFY_KEY == (int)fileIdentity)
                    )
                    .Select(s => new FileViewModel()
                    {
                        ID = s.ID,
                        FileName = s.FILE_RANDOM_NM,
                        RealFileName = s.FILE_REL_NM,
                        FilePath = s.FILE_PATH,
                        FileUrl = s.URL_PATH,
                    })
                    .ToList();

            return fileList;
        }

        public void Dispose()
        {
            if (this.DB.Database.Connection.State == System.Data.ConnectionState.Open)
            {
                this.DB.Database.Connection.Close();
            }
            this.DB.Dispose();
            this.DB = null;
        }
    }
}