using OutWeb.ActionResults;
using OutWeb.Entities;
using OutWeb.Modules.FrontEnd;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    public class FeedController : Controller
    {
        public ContentResult Index()
        {
            var feed = GetFeedData();
            return new RssActionResult(feed);
        }

        private SyndicationFeed GetFeedData()
        {
            DBEnergy DB = new DBEnergy();

            var hostUrl = string.Format("{0}://{1}",
                    Request.Url.Scheme,
                    Request.Headers["host"]);

            var feed = new SyndicationFeed(
                "能源資訊網 -國內外新聞",
                "國內外新聞",
                new Uri(string.Concat(hostUrl, "/Rss/")));

            var items = new List<SyndicationItem>();

            var news = DB.新聞.OrderByDescending(x => x.主索引);
            NewsFrontModule module = new NewsFrontModule();
            foreach (var n in news)
            {
                PublicMethodRepository.HtmlDecode(n);
                string typeName = module.GetNewsTypeNameByID(n.分類代碼);
                var item = new SyndicationItem(
                    string.Concat(typeName, " - ", n.標題),
                   "",
                    new Uri(string.Concat(hostUrl, "/News/Content?id=", n.主索引)),
                    "ID",
                    DateTime.Now);
                item.Content = new CDataSyndicationContent(new TextSyndicationContent(HttpUtility.HtmlDecode(n.內容), TextSyndicationContentKind.Html));
                items.Add(item);

            }

            feed.Items = items;
            DB.Dispose();
            return feed;
        }
    }
}