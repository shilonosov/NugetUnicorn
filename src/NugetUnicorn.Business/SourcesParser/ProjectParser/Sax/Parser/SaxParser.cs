using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Xml;

using NugetUnicorn.Business.Extensions;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax.Parser
{
    public class SaxParser
    {
        private ContentHolder _contentHolder;

        public SaxParser()
        {
            _contentHolder = new ContentHolder();
        }

        public IObservable<SaxEvent> Parse(string fullPath, IScheduler scheduler)
        {
            return Observable.Create<SaxEvent>(x => scheduler.Schedule(() => ParseInternal(fullPath, x)));
        }

        private void ParseInternal(string fullPath, IObserver<SaxEvent> x)
        {
            try
            {
                using (var reader = new XmlTextReader(fullPath))
                {
                    while (reader.Read())
                    {
                        ProcessNode(x, reader);
                    }
                }
                x.OnCompleted();
            }
            catch (XmlException e)
            {
                x.OnError(e);
            }
        }

        private void ProcessNode(IObserver<SaxEvent> x, XmlTextReader reader)
        {
            reader.NodeType
                  .Switch(true)
                  .Case(y => y == XmlNodeType.Element && reader.IsEmptyElement, y => HandleEndElement(reader, x))
                  .Case(y => y == XmlNodeType.Element, y => HandleStartElement(reader, x))
                  .Case(y => y == XmlNodeType.EndElement, y => HandleEndElement(reader, x))
                  .Case(y => y == XmlNodeType.Text, y => HandleTextElement(reader))
                  .Evaluate(reader.NodeType);
        }

        private void HandleTextElement(XmlTextReader reader)
        {
            _contentHolder.Append(new StringElementEvent(reader.Value));
        }

        private void HandleEndElement(XmlTextReader reader, IObserver<SaxEvent> observer)
        {
            var content = _contentHolder.GetContent();
            var isClosed = reader.IsEmptyElement;
            if (isClosed)
            {
                content = new List<SaxEvent>();
            }
            else
            {
                _contentHolder = _contentHolder.Parent;
            }

            var attributes = new Dictionary<string, string>();
            var strUri = reader.NamespaceURI;
            var strName = reader.Name;
            if (reader.HasAttributes)
            {
                for (var i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    attributes.Add(reader.Name, reader.Value);
                }
            }

            var readOnlyAttributes = new ReadOnlyDictionary<string, string>(attributes);
            var endElementEvent = new EndElementEvent(strUri, strName, isClosed, readOnlyAttributes, content);

            _contentHolder.Append(endElementEvent);

            observer.OnNext(endElementEvent);
        }

        private void HandleStartElement(XmlTextReader reader, IObserver<SaxEvent> observer)
        {
            _contentHolder = new ContentHolder(_contentHolder);

            var attributes = new Dictionary<string, string>();
            var strUri = reader.NamespaceURI;
            var strName = reader.Name;
            var isClosed = reader.IsEmptyElement;
            if (reader.HasAttributes)
            {
                for (var i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    attributes.Add(reader.Name, reader.Value);
                }
            }
            observer.OnNext(new StartElementEvent(strUri, strName, isClosed, new ReadOnlyDictionary<string, string>(attributes)));
        }
    }
}