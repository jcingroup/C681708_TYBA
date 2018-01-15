using OutWeb.Entities;
using OutWeb.Models.Manage.EditorModels;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Modules.Manage
{
    public class EditorModule : IDisposable
    {
        private DBEnergy m_DB = new DBEnergy();

        private DBEnergy DB
        { get { return this.m_DB; } set { this.m_DB = value; } }

        public void Dispose()
        {
            if (this.DB.Database.Connection.State == System.Data.ConnectionState.Open)
            {
                this.DB.Database.Connection.Close();
            }
            this.DB.Dispose();
            this.DB = null;
        }

        public TModel DoGetDetails<TModel>(ref TModel model) where TModel : Editor
        {
            string acName = model.MappingActionName;
            var result = this.DB.編輯器.FirstOrDefault(o => o.對應名稱 == acName);

            if (result != null)
            {
                model.Content = result.內容;
                PublicMethodRepository.HtmlDecode(model);
            }
            return model;
        }

        public void DoSaveData(FormCollection form, List<HttpPostedFileBase> files)
        {
            string acName = form["actionName"];
            FileRepository fileRepository = new FileRepository();
            編輯器 agent = new 編輯器();
            //this.DB.Database.ExecuteSqlCommand("DELETE WBAGENT");
            agent.對應名稱 = acName;
            agent.內容 = form["contenttext"];
            PublicMethodRepository.FilterXss(agent);

            var del = this.DB.編輯器.FirstOrDefault(o => o.對應名稱 == acName);
            if (del != null)
                this.DB.編輯器.Remove(del);
            this.DB.編輯器.Add(agent);
            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            #region 檔案處理

            List<int> oldFileList = new List<int>();

            #region 將原存在的Server檔案保留 記錄檔案ID

            //將原存在的Server檔案保留 記錄檔案ID
            foreach (var f in form.Keys)
            {
                if (f.ToString().StartsWith("FileData"))
                {
                    var id = Convert.ToInt16(form[f.ToString().Split('.')[0] + ".ID"]);
                    if (!oldFileList.Contains(id))
                        oldFileList.Add(id);
                }
            }

            #endregion 將原存在的Server檔案保留 記錄檔案ID

            #region 建立檔案模型

            FilesModel fileModel = new FilesModel()
            {
                ActionName = acName,
                ID = 1,
                OldFileIds = oldFileList
            };

            #endregion 建立檔案模型

            #region 若有null則是前端html的name重複於ajax formData名稱

            if (files != null)
            {
                if (files.Count > 0)
                    files.RemoveAll(item => item == null);
            }

            #endregion 若有null則是前端html的name重複於ajax formData名稱

            #region img data binding 單筆多筆裝在不同容器

            //fileModel.UploadType = "files_m";
            fileRepository.UploadFile("Post", fileModel, files, "M");
            fileRepository.SaveFileToDB(fileModel);

            #endregion img data binding 單筆多筆裝在不同容器

            #endregion 檔案處理
        }
    }
}