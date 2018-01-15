using Newtonsoft.Json;
using OutWeb.ActionFilter;
using OutWeb.Authorize;
using OutWeb.Models.Manage.ImgModels;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [ErrorHandler]
    public class ImgHandlerController : Controller
    {
        private ImageRepository m_repository = new ImageRepository();
        private ImageRepository Repository
        { get { return this.m_repository; } }

        /// <summary>
        /// 儲存圖檔
        /// </summary>
        /// <param name="act"></param>
        /// <param name="vm"></param>
        /// <param name="myFile"></param>
        /// <returns></returns>
        [CheckFolder]
        [HttpPost]
        public string Index(string act, ImagesModel vm, List<HttpPostedFileBase> image, List<HttpPostedFileBase> images)
        {
            this.Repository.UploadPhoto(act, vm, image, "S");
            this.Repository.UploadPhoto(act, vm, images, "M");
            if (image == null)
                vm.MemberData.Clear();
            if (images == null)
                vm.MemberDataMultiple.Clear();
            if (act == "post")
                this.Repository.SaveImagesToDB(vm);

            var jsonStrModel = JsonConvert.SerializeObject(vm);
            return jsonStrModel;
        }

        /// <summary>
        /// 刪除圖檔
        /// </summary>
        /// <param name="imgID"></param>
        /// <returns></returns>
        //[HttpPost]
        //public JsonResult DeleteImg(int imgID)
        //{
        //    bool success = true;
        //    string messages = string.Empty;
        //    JsonResult resultJson = new JsonResult();
        //    ImgModule imgModule = new ImgModule();
        //    try
        //    {
        //        imgModule.DeleteImg(imgID);
        //    }
        //    catch (Exception ex)
        //    {
        //        messages = string.Format("圖片刪除失敗 ：{0}", ex.Message);
        //        resultJson = Json(new { success = success, messages = messages });
        //        return resultJson;
        //    }
        //    resultJson = Json(new { success = success, messages = "success" });
        //    return resultJson;
        //}
    }
}