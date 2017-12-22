using OutWeb.Entities;
using OutWeb.Models.FrontEnd.CourseFrontEndModels;
using OutWeb.Repositories;
using System;
using System.Linq;

namespace OutWeb.Modules.FrontEnd
{
    public class CourseFrontModule : IDisposable
    {
        private DBEnergy DB = new DBEnergy();

        public CourseListFrontResultModel GetList()
        {
            CourseListFrontResultModel result = new CourseListFrontResultModel();
            try
            {
                var data = this.DB.課程
                                    .ToList()
                                    .OrderByDescending(o => o.排序).ThenBy(o => o.發稿日期)
                                    .Where(o => o.顯示狀態 == true)
                                    .Select(o => new CourseFrontListDataModel()
                                    {
                                        ID = o.主索引,
                                        Title = o.標題,
                                        UrlAddr = o.連結位址,
                                        Sort = o.排序,
                                        PublishDate = o.發稿日期
                                    })
                                    .ToList();
                foreach (var d in data)
                    PublicMethodRepository.HtmlDecode(d);

                data = data.OrderByDescending(o => o.Sort).ThenByDescending(o => o.PublishDate).ToList();
                result.Data = data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
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