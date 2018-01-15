using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OutWeb.Models.FrontEnd.UserInfo
{
    public class LogInModel
    {
        [Required(ErrorMessage = "請輸入登入帳號")]
        public string Account { get; set; }
        [Required(ErrorMessage = "請輸入登入密碼")]
        public string Password { get; set; }
    }
}