using System.Collections.Generic;
using OutWeb.Models.Manage;
namespace OutWeb.Models.Manage
{
    public interface IManage
    {
         List<MemberViewModel> FilesData { get; set; }

         List<MemberViewModel> ImagesData { get; set; }
    }
}