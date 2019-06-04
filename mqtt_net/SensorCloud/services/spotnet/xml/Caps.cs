using System.Collections.Generic;
using System.Xml.Serialization;

namespace SensorCloud.services.spotnet.xml.caps
{
    [XmlRoot(ElementName = "server")]
    public class Server
    {
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "strapline")]
        public string Strapline { get; set; }
        [XmlAttribute(AttributeName = "email")]
        public string Email { get; set; }
        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
        [XmlAttribute(AttributeName = "image")]
        public string Image { get; set; }
    }

    [XmlRoot(ElementName = "limits")]
    public class Limits
    {
        [XmlAttribute(AttributeName = "max")]
        public string Max { get; set; }
        [XmlAttribute(AttributeName = "default")]
        public string Default { get; set; }
    }

    [XmlRoot(ElementName = "retention")]
    public class Retention
    {
        [XmlAttribute(AttributeName = "days")]
        public string Days { get; set; }
    }

    [XmlRoot(ElementName = "registration")]
    public class Registration
    {
        [XmlAttribute(AttributeName = "available")]
        public string Available { get; set; }
        [XmlAttribute(AttributeName = "open")]
        public string Open { get; set; }
    }

    [XmlRoot(ElementName = "search")]
    public class Search
    {
        [XmlAttribute(AttributeName = "available")]
        public string Available { get; set; }
    }

    [XmlRoot(ElementName = "tv-search")]
    public class Tvsearch
    {
        [XmlAttribute(AttributeName = "available")]
        public string Available { get; set; }
    }

    [XmlRoot(ElementName = "movie-search")]
    public class Moviesearch
    {
        [XmlAttribute(AttributeName = "available")]
        public string Available { get; set; }
    }

    [XmlRoot(ElementName = "searching")]
    public class Searching
    {
        [XmlElement(ElementName = "search")]
        public Search Search { get; set; }
        [XmlElement(ElementName = "tv-search")]
        public Tvsearch Tvsearch { get; set; }
        [XmlElement(ElementName = "movie-search")]
        public Moviesearch Moviesearch { get; set; }
    }

    [XmlRoot(ElementName = "subcat")]
    public class Subcat
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "category")]
    public class Category
    {
        [XmlElement(ElementName = "subcat")]
        public List<Subcat> Subcat { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "categories")]
    public class Categories
    {
        [XmlElement(ElementName = "category")]
        public List<Category> Category { get; set; }
    }

    [XmlRoot(ElementName = "caps")]
    public class Caps
    {
        [XmlElement(ElementName = "server")]
        public Server Server { get; set; }
        [XmlElement(ElementName = "limits")]
        public Limits Limits { get; set; }
        [XmlElement(ElementName = "retention")]
        public Retention Retention { get; set; }
        [XmlElement(ElementName = "registration")]
        public Registration Registration { get; set; }
        [XmlElement(ElementName = "searching")]
        public Searching Searching { get; set; }
        [XmlElement(ElementName = "categories")]
        public Categories Categories { get; set; }
    }

}
