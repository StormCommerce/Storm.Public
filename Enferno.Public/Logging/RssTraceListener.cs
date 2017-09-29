using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Enferno.Public.Logging
{
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class RSSTraceListener : CustomTraceListener, IDisposable
    {
        public string TITLE
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Attributes["SyndicationTitle"]))
                    throw new ApplicationException("Attribute SyndicationTitle is missing in RSSTraceListener");
                return base.Attributes["SyndicationTitle"];
            }
        }
        public string DESCRIPTION
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Attributes["SyndicationDescription"]))
                    throw new ApplicationException("Attribute SyndicationDescription is missing in RSSTraceListener");
                return base.Attributes["SyndicationDescription"];
            }
        }
        public string ALTERNATELINK
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Attributes["SyndicationAlternateLink"]))
                    throw new ApplicationException("Attribute SyndicationAlternateLink is missing in RSSTraceListener");
                return base.Attributes["SyndicationAlternateLink"];
            }
        }
        public string AUTHORS
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Attributes["SyndicationAuthors"]))
                    throw new ApplicationException("Attribute SyndicationAuthors is missing in RSSTraceListener");
                return base.Attributes["SyndicationAuthors"];
            }
        }
        public string CATEGORIES
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Attributes["SyndicationCategory"]))
                    throw new ApplicationException("Attribute SyndicationCategory is missing in RSSTraceListener");
                return base.Attributes["SyndicationCategory"];
            }
        }
        public string ITEMLINK
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Attributes["SyndicationItemLink"]))
                    throw new ApplicationException("Attribute SyndicationItemLink is missing in RSSTraceListener");
                return base.Attributes["SyndicationItemLink"];
            }
        }
        public string FILEPATH
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Attributes["SyndicationFilePath"]))
                    throw new ApplicationException("Attribute SyndicationFilePath is missing in RSSTraceListener");
                return base.Attributes["SyndicationFilePath"];
            }
        }

        public SyndicationFeed Feed { get; set; }

        public RSSTraceListener() : base()
        {
        }

        public void initialize()
        {
            if (Feed != null)
                return;
            if (System.IO.File.Exists(FILEPATH))
            {
                var reader = XmlReader.Create(System.IO.File.OpenRead(FILEPATH));
                Feed = SyndicationFeed.Load(reader);
                reader.Close();
            }
            else
            {
                Feed = new SyndicationFeed(
                    title: TITLE,
                    description: DESCRIPTION,
                    feedAlternateLink: new Uri(ALTERNATELINK)
                );

                if (!string.IsNullOrWhiteSpace(AUTHORS))
                {
                    string[] authors = AUTHORS.Split(';');
                    foreach (var item in authors)
                    {
                        Feed.Authors.Add(new SyndicationPerson(item));
                    }
                }
                if (!string.IsNullOrWhiteSpace(CATEGORIES))
                {
                    string[] categories = CATEGORIES.Split(';');
                    foreach (var item in categories)
                    {
                        Feed.Categories.Add(new SyndicationCategory(item));
                    }
                }
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            initialize();
            var entry = data as LogEntry;
            if (data != null && this.Formatter != null)
            {
                var item = new SyndicationItem();
                foreach (var category in entry.Categories)
                {
                    item.Categories.Add(new SyndicationCategory(category));
                }

                item.Content = new TextSyndicationContent(this.Formatter.Format(entry));
                item.Id = entry.ActivityIdString;
                item.Links.Add(new SyndicationLink(new Uri(ITEMLINK)));
                item.PublishDate = entry.TimeStamp;
                //item.Summary = "";
                item.Title = new TextSyndicationContent(entry.Title);

                Feed.Items = new SyndicationItem[1] { item };
                //Feed.SaveAsRss20(XmlWriter);


                WriteToFile();

            }
        }

        public override void Write(string message)
        {
            initialize();
            var item = new SyndicationItem(message, string.Empty, new Uri(ITEMLINK));
            Feed.Items = new SyndicationItem[1] { item };
            //Feed.SaveAsRss20(XmlWriter);
            WriteToFile();

        }

        public override void WriteLine(string message)
        {
            this.Write(message);
        }

        XmlWriter _XmlWriter;
        public XmlWriter XmlWriter
        {
            get
            {
                if (_XmlWriter == null) _XmlWriter = XmlWriter.Create(FILEPATH);
                return _XmlWriter;
            }
        }

        private void WriteToFile()
        {
            var formatter = new Rss20FeedFormatter(Feed);

            formatter.WriteTo(XmlWriter);
            XmlWriter.Flush();
        }

        private void CleanUpFile(int keepNumberOfPosts)
        {
            throw new NotImplementedException();
        }

        public new void Dispose()
        {
            if (XmlWriter != null) XmlWriter.Close();
            base.Dispose();
        }
    }
}
