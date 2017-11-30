using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LOLVideo.Models;
using LOLVideo.Helper;
using LOLVideo.Videos._178;

namespace LOLVideo.Videos.LOL178
{
    public class LOL178Album : Album
    {
        public LOL178Album(string url)
            : base(url)
        {
            urlList.Add(url);

        }

        private List<string> urlList = new List<string>();

        protected override List<string> GetPageUrlList()
        {
            string macth = "_\\d*";
            int index = this.Url.IndexOf(".html");
            macth = this.Url.Insert(index, macth);


            Regex regex = new Regex(macth);
            var ms = regex.Matches(CurrentPageHtml);
            foreach (Match m in ms)
            {
                if (!urlList.Contains(m.Value))
                    urlList.Add(m.Value);
            }



            return urlList;
        }


        protected override Video CreateVideoInfo()
        {
            return new Lol178Video();
        }

        protected override string GetMatchRegex()
        {
            return "<dt><a href=\"(?<link>.*?)\".*title=\"(?<name>.*?)\".*background-image:[\\s]*url\\(\\'(?<img>.*?)\\'\\);\"><span>.*</span>[\\s]*(<strong>(?<time>.*)</strong>)?";
        }
    }

    public class Wow178Album : LOL178Album
    {
        public Wow178Album(string url)
            : base(url)
        {
           

        }
        protected override string GetMatchRegex()
        {
            return "<a class=\"imgs\" href=\"(?<link>http://wow.178.com/\\d+/\\d+.html)\" title=\"(?<name>.*?)\".*?<img src=\"(?<img>.*?)\"";
        }
    }
}
