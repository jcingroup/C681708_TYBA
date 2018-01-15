using Newtonsoft.Json;
using NLog;
using OutWeb.Models.Manage.SerializationModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OutWeb.Controllers.api
{
    [RoutePrefix("api/Utility")]
    public class UtilityController : ApiController
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 提供排程呼叫取得目前要發送的信件
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetMail")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetMail()
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath("~/MailJson");
                var get_files = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly).Where(x => x.EndsWith(".json", StringComparison.CurrentCultureIgnoreCase));
                IList<MailStruc> ms = new List<MailStruc>();
                foreach (var get_file in get_files)
                {
                    string text = await Task.Run(() => { return File.ReadAllText(get_file); });
                    var mailstruc = JsonConvert.DeserializeObject<MailStruc>(text);
                    if ((mailstruc.sendresult == null || mailstruc.sendresult.LastOrDefault() == null) || (mailstruc.sendresult != null && mailstruc.sendresult.LastOrDefault() != null && mailstruc.sendresult.LastOrDefault().result == false))
                        ms.Add(mailstruc);

                }
                return Ok(ms);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// 提供排程器呼叫，並寫入相關資訊，如果狀態是成功則移動JSON檔至備份資料夾。
        /// </summary>
        [HttpGet]
        [Route("SetMailState")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SetMailState([FromUri] SetMailStateParam p)
        {
            var id = p.id;
            var result = p.result;
            var r = new ReturnQuery();
            var file_path = HttpContext.Current.Server.MapPath("~/MailJson/" + id + ".json");
            if (File.Exists(file_path))
            {
                string text = File.ReadAllText(file_path);
                MailStruc mailstruc = JsonConvert.DeserializeObject<MailStruc>(text);
                if (mailstruc.sendresult == null)
                    mailstruc.sendresult = new List<SendResult>();

                mailstruc.sendresult.Add(new SendResult() { datetime = DateTime.Now, result = result });

                var json = JsonConvert.SerializeObject(mailstruc);
                await Task.Run(() => File.WriteAllText(file_path, json, System.Text.Encoding.UTF8));

                if (result)
                {
                    var move_path = HttpContext.Current.Server.MapPath("~/MailJson/finish/" + id + ".json");
                    File.Move(file_path, move_path);
                }
                r.status = 0;
            }
            else
            {
                r.status = 1;
                r.message = "檔案不存在";
            }
            return Ok(r);
        }

        public class SetMailStateParam
        {
            public string id { get; set; }
            public bool result { get; set; }
        }

        public class ReturnQuery
        {
            public int status { get; set; }
            public string message { get; set; }
        }
    }
}