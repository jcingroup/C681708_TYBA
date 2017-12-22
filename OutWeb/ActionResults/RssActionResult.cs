using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace OutWeb.ActionResults
{
    public class RssActionResult : ContentResult
    {
        private readonly SyndicationFeed feed;

        public RssActionResult()
        {
        }

        public RssActionResult(SyndicationFeed feed)
        {
            feed.Language = "zh-TW";
            this.feed = feed;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "text/xml";
            Rss20FeedFormatter formatter = new Rss20FeedFormatter(feed);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Encoding = Encoding.UTF8;
            xmlWriterSettings.Indent = true;
            TextWriter output = context.HttpContext.Response.Output;
            using (var writer = XmlWriter.Create(context.HttpContext.Response.Output, xmlWriterSettings))
            {
                formatter.WriteTo(writer);
            }
        }


    }
    public class CDataSyndicationContent : TextSyndicationContent
    {
        public CDataSyndicationContent(TextSyndicationContent content)
            : base(content)
        {
        }

        protected override void WriteContentsTo(System.Xml.XmlWriter writer)
        {
            writer.WriteCData(Text);
        }
    }
}