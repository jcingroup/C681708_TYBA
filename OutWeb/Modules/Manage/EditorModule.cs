using OutWeb.Entities;
using OutWeb.Models.Manage.FileModels;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OutWeb.Modules.Manage
{
    public class EditorModule : IDisposable
    {
        private TYBADB m_DB = new TYBADB();

        private TYBADB DB
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

        public string GetContent()
        {
            string result = string.Empty;

            var data = this.DB.EDITOR.FirstOrDefault();

            if (data != null)
            {
                result = data.CONTENT;
                PublicMethodRepository.HtmlDecode(new List<string>() { result });
            }
            return result;
        }

        public void SaveContent(string content)
        {
            FileRepository fileRepository = new FileRepository();
            EDITOR agent = null;
            var @base = DB.EDITOR.FirstOrDefault();

            if (@base == null)
                agent = new EDITOR() { CONTENT = content };
            else
            {
                @base.CONTENT = content;
                agent = @base;
            }

            PublicMethodRepository.FilterXss(agent);

            if (@base == null)
                this.DB.EDITOR.Add(agent);
            else
                this.DB.Entry(agent).State = EntityState.Modified;
            try
            {
                this.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}