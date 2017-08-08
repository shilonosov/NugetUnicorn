using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Xml;
using NugetUnicorn.Utils.Extensions;

namespace NugetUnicorn.Utils.Sax.Parser
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
            return Observable.Create<SaxEvent>(x => scheduler.Schedule(() => { ParseInternal(fullPath, x); }));
        }

        public IObservable<SaxEvent> Parse(TextReader textReader, IScheduler scheduler)
        {
            return Observable.Create<SaxEvent>(x => scheduler.Schedule(() => { ParseInternal(textReader, x); }));
        }

        private void ParseInternal(string fullPath, IObserver<SaxEvent> x)
        {
            using (var inputStream = new StreamReader(fullPath))
            {
                ParseInternal(inputStream, x);
            }
        }

        private void ParseInternal(TextReader textReader, IObserver<SaxEvent> x)
        {
            try
            {
                using (var reader = new XmlTextReader(textReader))
                {
                    var elements = new Stack<string>();
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
                        {
                            elements.Push(reader.Name);
                        }
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            elements.Pop();
                        }
                        ProcessNode(x, reader, elements.Reverse().ToArray());
                    }
                }
                x.OnCompleted();
            }
            catch (XmlException e)
            {
                x.OnError(e);
            }
        }

        private void ProcessNode(IObserver<SaxEvent> x, XmlTextReader reader, string[] path)
        {
            reader.NodeType
                  .Switch<XmlNodeType, Unit>()
                  .Case(y => y == XmlNodeType.Element && reader.IsEmptyElement, y => HandleEmptyElement(reader, x, path))
                  .Case(y => y == XmlNodeType.Element, y => HandleStartElement(reader, x, path))
                  .Case(y => y == XmlNodeType.EndElement, y => HandleEndElement(reader, x, path))
                  .Case(y => y == XmlNodeType.Text, y => HandleTextElement(reader, path))
                  .Evaluate();
        }

        private Unit HandleEmptyElement(XmlTextReader reader, IObserver<SaxEvent> observer, string[] path)
        {
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
            var endElementEvent = new EndElementEvent(strUri, strName, true, readOnlyAttributes, new List<SaxEvent>(), path);

            _contentHolder.Append(endElementEvent);
            observer.OnNext(endElementEvent);

            return Unit.Default;
        }

        private Unit HandleTextElement(XmlTextReader reader, string[] path)
        {
            _contentHolder.Append(new StringElementEvent(reader.Value, path));
            return Unit.Default;
        }

        private Unit HandleEndElement(XmlTextReader reader, IObserver<SaxEvent> observer, string[] path)
        {
            var content = _contentHolder.GetContent();
            var startElement = _contentHolder.StartSaxEvent;
            _contentHolder = _contentHolder.Parent;

            var strUri = reader.NamespaceURI;
            var strName = reader.Name;
            var endElementEvent = new EndElementEvent(strUri, strName, false, startElement.Attributes, content, path);

            _contentHolder.Append(endElementEvent);

            observer.OnNext(endElementEvent);

            return Unit.Default;
        }

        private Unit HandleStartElement(XmlTextReader reader, IObserver<SaxEvent> observer, string[] path)
        {
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
            var startElementEvent = new StartElementEvent(strUri, strName, isClosed, new ReadOnlyDictionary<string, string>(attributes), path);
            _contentHolder = new ContentHolder(startElementEvent, _contentHolder);
            observer.OnNext(startElementEvent);

            return Unit.Default;
        }
    }
}