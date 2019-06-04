using System.Collections.Generic;
using System.Xml.Serialization;

namespace SensorCloud.services.spotnet.xml.rss
{
    [XmlRoot(ElementName = "response", Namespace = "http://www.newznab.com/DTD/2010/feeds/attributes/")]
    public class Response
    {
        [XmlAttribute(AttributeName = "offset")]
        public string Offset { get; set; }
        [XmlAttribute(AttributeName = "total")]
        public string Total { get; set; }
    }

    [XmlRoot(ElementName = "guid")]
    public class Guid
    {
        [XmlAttribute(AttributeName = "isPermaLink")]
        public string IsPermaLink { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "enclosure")]
    public class Enclosure
    {
        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
        [XmlAttribute(AttributeName = "length")]
        public long Length { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "attr", Namespace = "http://www.newznab.com/DTD/2010/feeds/attributes/")]
    public class Attr
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public long Value { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class Item
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "guid")]
        public Guid Guid { get; set; }
        [XmlElement(ElementName = "link")]
        public string Link { get; set; }
        [XmlElement(ElementName = "pubDate")]
        public string PubDate { get; set; }
        [XmlElement(ElementName = "enclosure")]
        public Enclosure Enclosure { get; set; }
        [XmlElement(ElementName = "attr", Namespace = "http://www.newznab.com/DTD/2010/feeds/attributes/")]
        public List<Attr> Attr { get; set; }
    }

    [XmlRoot(ElementName = "channel")]
    public class Channel
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "response", Namespace = "http://www.newznab.com/DTD/2010/feeds/attributes/")]
        public Response Response { get; set; }
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }
    }

    [XmlRoot(ElementName = "rss")]
    public class Rss
    {
        [XmlElement(ElementName = "channel")]
        public Channel Channel { get; set; }
        [XmlAttribute(AttributeName = "newznab", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Newznab { get; set; }
    }
}
