using NNTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpotNet
{
    public class Spot
    {
        public long article;
        public DateTime posted;
        public string articleid;
        public string title;
        public string cat;
        public string subcat = "";
        public long size;
        public string desc;
        public List<string> segments;


        public Spot(Header header)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(header.headers["X-XML"]);
            try
            {
                this.article = header.articleNumber;
                this.articleid = header.articleId;

                var data = xml["Spotnet"];
                if (data == null)
                    data = xml["SpotNet"];

                this.title = data["Posting"]["Title"].FirstChild.Value;
                this.desc = data["Posting"]["Description"].FirstChild.Value;
                this.cat = data["Posting"]["Category"].FirstChild.Value;
                this.posted = DateTime.UnixEpoch.AddSeconds(int.Parse(data["Posting"]["Created"].FirstChild.Value));
                if (data["Posting"]["Size"] != null)
                    this.size = long.Parse(data["Posting"]["Size"].FirstChild.Value);
                this.segments = new List<string>();
                if (data["Posting"]["NZB"] != null)
                    foreach (var e in data["Posting"]["NZB"])
                    {
                        XmlElement el = e as XmlElement;
                        segments.Add(el.FirstChild.Value);
                    }
                if (data["Posting"]["Message-ID"] != null)
                {
                    segments.Add(data["Posting"]["Message-ID"].FirstChild.Value);
                }
            }
            catch(NullReferenceException e)
            {
                Console.WriteLine(e);
            }
        }

        public override string ToString()
        {
            return $"Article:\t\t{article}\n" +
                $"Posted:\t\t{posted}\n" +
                $"ArticleId:\t{articleid}\n" +
                $"Title:\t\t{title}\n" +
                $"Cat:\t\t{cat}\n" +
                $"size:\t\t{size}\n" +
                $"Desc:\t\t{desc}\n" +
                $"Segments:\t{segments}";
        }
    }
}
