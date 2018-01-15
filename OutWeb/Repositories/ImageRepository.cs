using OutWeb.Models.Manage;
using OutWeb.Models.Manage.ImgModels;
using OutWeb.Modules.Manage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Web;

namespace OutWeb.Repositories
{
    public class ImageRepository
    {
        private int imgMaxWidth { get; set; }
        private int imgMaxHeight { get; set; }
        private string savePath { get; set; }

        public ImageRepository()
        {
        }

        public ImageRepository(int maxWidth, int maxHeight, string savePath)
        {
            this.imgMaxWidth = maxWidth;
            this.imgMaxHeight = maxHeight;
            this.savePath = savePath;
        }

        #region 儲存處理函式

        /// <summary>
        /// 寫Log查看表單post的結果
        /// </summary>
        /// <param name="vm"></param>
        public void SaveImagesToDB(ImagesModel vm)
        {
            ImgModule imgModule = new ImgModule();
            imgModule.SaveImgs(vm);
        }

        /// <summary>
        /// 上傳照片
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="myFile"></param>
        public void UploadPhoto(string uploadType, ImagesModel vm, List<HttpPostedFileBase> images, string mode)
        {
            string serverMapPath = string.Empty;

            if (uploadType == "upload")
                serverMapPath = "~/Content/Upload/Manage/Images/Temp/";
            else
                serverMapPath = "~/Content/Upload/Manage/Images/";
            if (mode == "S")
            {
                foreach (var m in vm.MemberData)
                    m.FilePath = HttpContext.Current.Server.MapPath(serverMapPath + m.FileName);
            }
            else
            {
                foreach (var m in vm.MemberDataMultiple)
                    m.FilePath = HttpContext.Current.Server.MapPath(serverMapPath + m.FileName);
            }
            int imgMaxWidth = 0;
            int imgMaxHeight = 0;

            switch (mode)
            {
                case "S":
                    imgMaxWidth = 400;
                    imgMaxHeight = 300;
                    break;

                case "M":
                    imgMaxWidth = 1000;
                    break;

                default:
                    break;
            }

            if (images != null && images.Count > 0)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    string strFileName = uploadType == "upload" ? images[i].FileName : Guid.NewGuid().ToString() + Path.GetExtension(images[i].FileName);
                    string strFilePath = HttpContext.Current.Server.MapPath(serverMapPath + strFileName);
                    string realFileName = images[i].FileName;
                    if (uploadType.ToUpper() == "POST")
                    {
                        #region 圖片尺寸調整

                        ImageRepository imageRepository = new ImageRepository(imgMaxWidth, imgMaxHeight, strFilePath);
                        Image img = Image.FromStream(images[i].InputStream);
                        imageRepository.SaveAdjustImageSize((Bitmap)img);

                        #endregion 圖片尺寸調整
                    }
                    else
                    {
                        images[i].SaveAs(strFilePath);
                    }

                    #region data binding to model

                    if (mode == "S")
                    {
                        vm.MemberData.Add(new MemberViewModel()
                        {
                            RealFileName = realFileName,
                            FilePath = strFilePath,
                            FileName = strFileName,
                            FileUrl = serverMapPath.Substring(2, serverMapPath.Length - 2) + strFileName,
                        });
                    }
                    else if (mode == "M")
                    {
                        vm.MemberDataMultiple.Add(new MemberViewModel()
                        {
                            RealFileName = realFileName,
                            FilePath = strFilePath,
                            FileName = strFileName,
                            FileUrl = serverMapPath.Substring(2, serverMapPath.Length - 2) + strFileName,
                        });
                    }

                    #endregion data binding to model
                }
            }
        }

        #endregion 儲存處理函式

        /// <summary>
        /// 調整圖片大小並儲存(等比例縮圖)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void SaveAdjustImageSize(Bitmap image)
        {
            if (image.Width > imgMaxWidth)
                this.ThumbPicWidth(image, this.imgMaxWidth);
            else if (imgMaxHeight > 0 && image.Height > 0 && image.Height > imgMaxHeight)
                this.ThumbPicHeight(image, this.imgMaxHeight);
            else
                image.Save(this.savePath);
        }

        #region 取得圖片等比例縮圖後的寬和高像素

        /// <summary>
        ///  寬高誰較長就縮誰  - 計算方法
        /// </summary>
        /// <param name="image">System.Drawing.Image 的物件</param>
        /// <param name="maxPx">寬或高超過多少像素就要縮圖</param>
        /// <returns>回傳int陣列，索引0為縮圖後的寬度、索引1為縮圖後的高度</returns>
        private int[] GetThumbPic_WidthAndHeight(System.Drawing.Image image, int maxPx)
        {
            int fixWidth = 0;

            int fixHeight = 0;

            if (image.Width > maxPx || image.Height > maxPx)
            //如果圖片的寬大於最大值或高大於最大值就往下執行
            {
                if (image.Width >= image.Height)
                //圖片的寬大於圖片的高
                {
                    fixHeight = Convert.ToInt32((Convert.ToDouble(maxPx) / Convert.ToDouble(image.Width)) * Convert.ToDouble(image.Height));
                    //設定修改後的圖高
                    fixWidth = maxPx;
                }
                else
                {
                    fixWidth = Convert.ToInt32((Convert.ToDouble(maxPx) / Convert.ToDouble(image.Height)) * Convert.ToDouble(image.Width));
                    //設定修改後的圖寬
                    fixHeight = maxPx;
                }
            }
            else
            {//圖片沒有超過設定值，不執行縮圖
                fixHeight = image.Height;

                fixWidth = image.Width;
            }

            int[] fixWidthAndfixHeight = { fixWidth, fixHeight };

            return fixWidthAndfixHeight;
        }

        /// <summary>
        /// 寬度維持maxWidth，高度等比例縮放   - 計算方法
        /// </summary>
        /// <param name="image"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        private static int[] GetThumbPic_Width(Image image, int maxWidth)
        {
            //要回傳的結果
            int fixWidth = 0;
            int fixHeight = 0;

            if (image.Width > maxWidth)
            //如果圖片的寬大於最大值
            {
                //等比例的圖高
                fixHeight = Convert.ToInt32((Convert.ToDouble(maxWidth) / Convert.ToDouble(image.Width)) * Convert.ToDouble(image.Height));
                //設定修改後的圖寬
                fixWidth = maxWidth;
            }
            else
            {//圖片寬沒有超過設定值，不執行縮圖
                fixHeight = image.Height;

                fixWidth = image.Width;
            }

            int[] fixWidthAndfixHeight = { fixWidth, fixHeight };

            return fixWidthAndfixHeight;
        }

        /// <summary>
        /// 高度維持maxHeight，寬度等比例縮放  - 計算方法
        /// </summary>
        /// <param name="image"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        private static int[] GetThumbPic_Height(Image image, int maxHeight)
        {
            //要回傳的值
            int fixWidth = 0;
            int fixHeight = 0;

            if (image.Height > maxHeight)
            //如果圖片的高大於最大值
            {
                //等比例的寬
                fixWidth = Convert.ToInt32((Convert.ToDouble(maxHeight) / Convert.ToDouble(image.Height)) * Convert.ToDouble(image.Width));
                //圖高固定
                fixHeight = maxHeight;
            }
            else
            {//圖片的高沒有超過設定值
                fixHeight = image.Height;

                fixWidth = image.Width;
            }

            int[] fixWidthAndfixHeight = { fixWidth, fixHeight };

            return fixWidthAndfixHeight;
        }

        #endregion 取得圖片等比例縮圖後的寬和高像素

        #region 產生縮圖並儲存

        /// <summary>
        /// 產生縮圖並儲存 寬高誰較長就縮誰
        /// </summary>
        /// <param name="srcImagePath">來源圖片的路徑</param>
        /// <param name="maxPix">超過多少像素就要等比例縮圖</param>
        /// <param name="saveThumbFilePath">縮圖的儲存檔案路徑</param>
        private void SaveThumbPic(Bitmap bitmap, int maxPix)
        {
            //圖片寬高
            int ImgWidth = bitmap.Width;
            int ImgHeight = bitmap.Height;
            // 計算維持比例的縮圖大小
            int[] thumbnailScaleWidth = GetThumbPic_WidthAndHeight(bitmap, maxPix);
            int AfterImgWidth = thumbnailScaleWidth[0];
            int AfterImgHeight = thumbnailScaleWidth[1];

            // 產生縮圖
            using (var bmp = new Bitmap(AfterImgWidth, AfterImgHeight))
            {
                using (var gr = Graphics.FromImage(bmp))
                {
                    gr.CompositingQuality = CompositingQuality.HighQuality;
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.DrawImage(bitmap, new Rectangle(0, 0, AfterImgWidth, AfterImgHeight), 0, 0, ImgWidth, ImgHeight, GraphicsUnit.Pixel);
                    bmp.Save(this.savePath);
                }
            }
        }

        /// <summary>
        /// 產生縮圖並儲存 寬度維持maxpix，高度等比例
        /// </summary>
        /// <param name="srcImagePath">來源圖片的路徑</param>
        /// <param name="widthMaxPix">超過多少像素就要等比例縮圖</param>
        /// <param name="saveThumbFilePath">縮圖的儲存檔案路徑</param>
        private void ThumbPicWidth(Bitmap bitmap, int widthMaxPix)
        {
            //圖片寬高
            int ImgWidth = bitmap.Width;
            int ImgHeight = bitmap.Height;
            // 計算維持比例的縮圖大小
            int[] thumbnailScaleWidth = GetThumbPic_Width(bitmap, widthMaxPix);
            int AfterImgWidth = thumbnailScaleWidth[0];
            int AfterImgHeight = thumbnailScaleWidth[1];

            // 產生縮圖
            using (var bmp = new Bitmap(AfterImgWidth, AfterImgHeight))
            {
                using (var gr = Graphics.FromImage(bmp))
                {
                    gr.CompositingQuality = CompositingQuality.HighQuality;
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.DrawImage(bitmap, new Rectangle(0, 0, AfterImgWidth, AfterImgHeight), 0, 0, ImgWidth, ImgHeight, GraphicsUnit.Pixel);
                    bmp.Save(this.savePath);
                }
            }
        }

        /// <summary>
        /// 產生縮圖並儲存 高度維持maxPix，寬度等比例
        /// </summary>
        /// <param name="srcImagePath">來源圖片的路徑</param>
        /// <param name="heightMaxPix">超過多少像素就要等比例縮圖</param>
        /// <param name="saveThumbFilePath">縮圖的儲存檔案路徑</param>
        private Bitmap ThumbPicHeight(Bitmap bitmap, int heightMaxPix)
        {
            Bitmap resultBmp = null;
            //圖片寬高
            int ImgWidth = bitmap.Width;
            int ImgHeight = bitmap.Height;
            // 計算維持比例的縮圖大小
            int[] thumbnailScaleWidth = GetThumbPic_Height(bitmap, heightMaxPix);
            int AfterImgWidth = thumbnailScaleWidth[0];
            int AfterImgHeight = thumbnailScaleWidth[1];

            // 產生縮圖
            using (var bmp = new Bitmap(AfterImgWidth, AfterImgHeight))
            {
                using (var gr = Graphics.FromImage(bmp))
                {
                    gr.CompositingQuality = CompositingQuality.HighQuality;
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.DrawImage(bitmap, new Rectangle(0, 0, AfterImgWidth, AfterImgHeight), 0, 0, ImgWidth, ImgHeight, GraphicsUnit.Pixel);
                    resultBmp = bmp;
                    bmp.Save(this.savePath);
                }
            }
            return resultBmp;
        }

        #endregion 產生縮圖並儲存
    }
}