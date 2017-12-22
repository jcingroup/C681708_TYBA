using OutWeb.Entities;
using OutWeb.Models.FrontEnd.LinkFrontEndModels;
using OutWeb.Models.Manage;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutWeb.Modules.FrontEnd 
{
    public class LinkFrontModule :IDisposable
    {
        private DBEnergy DB = new DBEnergy();

        public LinkListFrontResultModel GetList()
        {
            LinkListFrontResultModel result = new LinkListFrontResultModel();
            try
            {
                var data = this.DB.外部連結
                                    .ToList()
                                    .OrderByDescending(o => o.排序).ThenBy(o => o.建立日期)
                                    .Where(o => o.狀態 == true)
                                    .Select(o => new LinkFrontListDataModel()
                                    {
                                        ID = o.主索引,
                                        Title = o.名稱,
                                        UrlAddr = o.網址連結,
                                        Image = GetImage((int)o.主索引)
                                    })
                                    .ToList();
                foreach (var d in data)
                    PublicMethodRepository.HtmlDecode(d);

                result.Data = data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        private List<MemberViewModel> GetImage(int ID)
        {
            List<MemberViewModel> images = new List<MemberViewModel>();

            //取圖檔
            ImgModule imgModule = new ImgModule();
            images = imgModule.GetImages(ID, "Links", "M");
            return images;
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