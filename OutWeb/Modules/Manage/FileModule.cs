using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Models.Manage;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Provider;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace OutWeb.Modules.Manage
{
    public class FileModule : IDisposable
    {
        private DBEnergy m_DB = new DBEnergy();
        private string rootPath { get { return HttpContext.Current.Server.MapPath("~/"); } }

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        /// <summary>
        /// 儲存圖片
        /// </summary>
        /// <param name="model"></param>
        public void SaveFiles(FilesModel model)
        {
            if (model.ID > 0)
            {
                List<檔案> filterRemove = new List<檔案>();
                if (model.UploadIdentify == FileUploadType.NOTSET)
                {
                    filterRemove = this.DB.檔案
                                        .Where(o => !model.OldFileIds.Contains(o.主索引) &&
                                        o.對應索引 == model.ID &&
                                        o.對應名稱 == model.ActionName &&
                                        o.檔案格式 == "F"
                                        ).ToList();
                }
                else
                {
                    filterRemove = this.DB.檔案
                   .Where(o => !model.OldFileIds.Contains(o.主索引) &&
                   o.對應索引 == model.ID &&
                   o.對應名稱 == model.ActionName &&
                   o.檔案格式 == "F" &&
                   o.識別碼 == (int)model.UploadIdentify
                   ).ToList();
                }

                if (filterRemove.Count > 0)
                {
                    foreach (var f in filterRemove)
                        File.Delete(string.Concat(rootPath, f.檔案路徑));
                    //刪除舊檔
                    this.DB.檔案.RemoveRange(filterRemove);
                    this.DB.SaveChanges();
                }
            }

            ////存檔單筆
            //foreach (var f in model.MemberData)
            //{
            //    if (model.ActionName.ToUpper().StartsWith("FAX"))
            //    {
            //        string faxFirstName = PublicMethodRepository.GetConfigAppSetting("FaxDocNamel");
            //        var faxLastNameWithRandom = new Random().Next(0, 999999).ToString().PadLeft(6, '0');
            //        f.RealFileName = string.Concat(faxFirstName, faxLastNameWithRandom);
            //    }

            //    檔案 file = new 檔案()
            //    {
            //        檔案名稱 = f.FileName,
            //        對應名稱 = model.ActionName,
            //        檔案模式 = "S",
            //        排序 = 0,
            //        更新時間 = DateTime.UtcNow.AddHours(8),
            //        更新人員 = UserProvider.Instance.User.ID,
            //        對應索引 = model.ID,
            //        原始檔名 = f.RealFileName,
            //        建立時間 = DateTime.UtcNow.AddHours(8),
            //        建立人員 = UserProvider.Instance.User.ID,
            //        檔案格式 = "F",
            //        檔案路徑 = f.FilePath,
            //        檔案虛擬路徑 = f.FileUrl,
            //        識別碼 = model.UploadIdentify == FileUploadType.NOTSET ? default(int?) : (int)model.UploadIdentify,
            //    };
            //    this.DB.檔案.Add(file);
            //    this.DB.SaveChanges();
            //}
            //存檔多筆
            foreach (var f in model.MemberDataMultiple)
            {
                int sq = model.MemberDataMultiple.IndexOf(f);
                檔案 file = new 檔案()
                {
                    檔案名稱 = f.FileName,
                    對應名稱 = model.ActionName,
                    檔案模式 = "M",
                    排序 = sq,
                    更新時間 = DateTime.UtcNow.AddHours(8),
                    更新人員 = UserProvider.Instance.User.ID,
                    對應索引 = model.ID,
                    原始檔名 = f.RealFileName,
                    建立時間 = DateTime.UtcNow.AddHours(8),
                    建立人員 = UserProvider.Instance.User.ID,
                    檔案格式 = "F",
                    檔案路徑 = f.FilePath,
                    檔案虛擬路徑 = f.FileUrl,
                    識別碼 = model.UploadIdentify == FileUploadType.NOTSET ? default(int?) : (int)model.UploadIdentify,
                };
                this.DB.檔案.Add(file);
                this.DB.SaveChanges();
                f.ID = file.主索引;
            }
        }

        /// <summary>
        /// 取得檔案
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="actionName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<MemberViewModel> GetFiles(int ID, string actionName, string actionMode, FileUploadType fileType = FileUploadType.NOTSET)
        {

            List<MemberViewModel> fileList = new List<MemberViewModel>();
            fileList = this.DB.檔案
                    .Where(o => o.對應索引 == ID
                    && o.對應名稱.StartsWith(actionName)
                    && o.檔案模式 == actionMode && o.檔案格式 == "F"
                    && (fileType == FileUploadType.NOTSET ? true : o.識別碼 == (int)fileType)
                    )
                    .Select(s => new MemberViewModel()
                    {
                        ID = s.主索引,
                        FileName = s.檔案名稱,
                        RealFileName = s.原始檔名,
                        FilePath = s.檔案路徑,
                        FileUrl = s.檔案虛擬路徑,
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