using OutWeb.Entities;
using OutWeb.Models.Manage;
using OutWeb.Models.Manage.ImgModels;
using OutWeb.Provider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OutWeb.Modules.Manage
{
    public class ImgModule:IDisposable
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        /// <summary>
        /// 儲存圖片
        /// </summary>
        /// <param name="model"></param>
        public void SaveImgs(ImagesModel model)
        {
            if (model.ID > 0)
            {
                List<檔案> imageRemove = new List<檔案>();
                imageRemove = this.DB.檔案
                    .Where(o => !model.OldImageIds.Contains(o.主索引) &&
                    o.對應索引 == model.ID &&
                    o.對應名稱 == model.ActionName &&
                    o.檔案格式 == "P" &&
                    o.識別碼 == null
                    )
                    .ToList();
                if (imageRemove.Count > 0)
                {
                    foreach (var img in imageRemove)
                        File.Delete(img.檔案路徑);
                    //刪除舊圖
                    this.DB.檔案.RemoveRange(imageRemove);
                    this.DB.SaveChanges();
                }
            }

            //存檔單筆
            foreach (var img in model.MemberData)
            {
                檔案 pic = new 檔案()
                {
                    檔案名稱 = img.FileName,
                    對應名稱 = model.ActionName,
                    檔案模式 = "S",
                    排序 = 0,
                    更新時間 = DateTime.UtcNow.AddHours(8),
                    更新人員 = UserProvider.Instance.User.ID,
                    對應索引 = model.ID,
                    原始檔名 = img.RealFileName,
                    建立時間 = DateTime.UtcNow.AddHours(8),
                    建立人員 = UserProvider.Instance.User.ID,
                    檔案格式 = "F",
                    檔案路徑 = img.FilePath,
                    檔案虛擬路徑 = img.FileUrl
                };
                this.DB.檔案.Add(pic);
                this.DB.SaveChanges();
            }
            //存檔多筆
            foreach (var img in model.MemberDataMultiple)
            {
                int sq = model.MemberDataMultiple.IndexOf(img);
                檔案 pic = new 檔案()
                {
                    檔案名稱 = img.FileName,
                    對應名稱 = model.ActionName,
                    檔案模式 = "M",
                    排序 = sq,
                    更新時間 = DateTime.UtcNow.AddHours(8),
                    更新人員 = UserProvider.Instance.User.ID,
                    對應索引 = model.ID,
                    原始檔名 = img.RealFileName,
                    建立時間 = DateTime.UtcNow.AddHours(8),
                    建立人員 = UserProvider.Instance.User.ID,
                    檔案格式 = "P",
                    檔案路徑 = img.FilePath,
                    檔案虛擬路徑 = img.FileUrl
                };
                this.DB.檔案.Add(pic);
                this.DB.SaveChanges();
                img.ID = pic.主索引;
            }
        }

        /// <summary>
        /// 取得圖片
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="actionName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<FileViewModel> GetImages(int ID, string actionName, string actionMode)
        {
            List<FileViewModel> imgList = new List<FileViewModel>();
            imgList = this.DB.檔案
                    .Where(o => o.對應索引 == ID && o.對應名稱.StartsWith(actionName) && o.檔案模式 == "M" && o.檔案格式 == "P")
                    .Select(s => new FileViewModel()
                    {
                        ID = s.主索引,
                        FileName = s.檔案名稱,
                        RealFileName = s.原始檔名,
                        FilePath = s.檔案路徑,
                        FileUrl = s.檔案虛擬路徑
                    })
                    .ToList();

            return imgList;
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


        /// <summary>
        /// 刪除圖片
        /// </summary>
        /// <param name="imgID"></param>
        //public void DeleteImg(int imgID)
        //{
        //    var img = this.DB.檔案.Where(o => o.主索引 == imgID).First();
        //    File.Delete(img.檔案路徑);
        //    this.DB.檔案.Remove(img);
        //    this.DB.SaveChanges();
        //}
    }
}