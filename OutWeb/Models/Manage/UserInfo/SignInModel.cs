using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OutWeb.Models.UserInfo
{
    public class SignInModel
    {
        [Required(ErrorMessage = "請輸入登入帳號")]
        public string Account { get; set; }
        [Required(ErrorMessage = "請輸入登入密碼")]
        public string Password { get; set; }
        [Required(ErrorMessage = "請輸入驗證碼")]
        public string CaptchaCode { get; set; }
   
    }
}