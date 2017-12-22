using System;
using System.Web.Mvc;
using MitakeSMS;
using System.Globalization;
using NLog;
using System.Configuration;

namespace OutWeb.Controllers
{
    //負責接收與回傳參數給三竹
    public class SMSResponseController : Controller
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        // GET: SMSResponse
        public void GetResponse()
        {

            string SMSResponseIP = ConfigurationManager.AppSettings["SMSResponseIP"];

            //-------------接回傳資料的IP位置--------------------
            string VisitorsIPAddr = string.Empty;
            if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                VisitorsIPAddr = Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else if (Request.UserHostAddress.Length != 0)
            {
                VisitorsIPAddr = Request.UserHostAddress;
            }

            //---------------------------------------------------

            //--------------接收三竹傳來的參數-------------------
            string msgid = this.Request.QueryString["msgid"];
            string dstaddr = this.Request.QueryString["dstaddr"];
            string dlvtime = this.Request.QueryString["dlvtime"];
            string donetime = this.Request.QueryString["donetime"];
            string statusstr = this.Request.QueryString["statusstr"];
            string statuscode = this.Request.QueryString["statuscode"];
            string statusFlag = this.Request.QueryString["StatusFlag"];
            //----------------------------------------------------

            logger.Info("接收三竹Response回傳參數=>msgid:{0}-dstaddr:{1}運行-dlvtime:{2}-donetime:{3}-statusstr:{4}-statuscode:{5}-StatusFlag:{6}，IP:{7}，預設三竹回傳IP為:{8}", msgid, dstaddr, dlvtime, donetime, statusstr, statuscode, statusFlag, VisitorsIPAddr,SMSResponseIP);

            MitakeSMSClass.SaveResponseParam rp = new MitakeSMSClass.SaveResponseParam();
            rp.msgid = msgid;
            rp.dstaddr = dstaddr;
            rp.statusstr = statusstr;
            rp.statuscode = Convert.ToInt32(statuscode);
            rp.statusFlag = Convert.ToInt32(statusFlag);
            rp.receiveDate = DateTime.UtcNow.AddHours(8);

            if (VisitorsIPAddr == SMSResponseIP) //判斷是否為三竹回傳的IP
            {
                try
                {
                    //-------時間字串轉換成DateTime格式--------------
                    DateTime parsedDlvTime;
                    DateTime.TryParseExact(dlvtime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDlvTime);
                    rp.donetime = parsedDlvTime;

                    DateTime parsedDoneTime;
                    DateTime.TryParseExact(dlvtime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDoneTime);
                    rp.dlvtime = parsedDoneTime;
                    //------------------------------------------------


                    OutWeb.Modules.Manage.SMSModule mdu = new Modules.Manage.SMSModule();
                    var success = mdu.SaveSMSResponseToDB(rp);

                    if (success)
                    {
                        //-----回傳值給三竹---------------------------
                        Response.Write("magicid=sms_gateway_rpack" + Convert.ToString((char)10) + "msgid=" + rp.msgid + Convert.ToString((char)10));
                        //--------------------------------------------
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }

        }

       
    }
}