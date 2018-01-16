using System.Collections.Generic;
using OutWeb.Models.Manage;
namespace OutWeb.Models.Manage
{
    public interface IManage
    {
         List<FileViewModel> FilesData { get; set; }

         List<FileViewModel> ImagesData { get; set; }
    }
}