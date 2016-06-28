using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Xml;

using NugetUnicorn.Business.Extensions;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
{
    public class ProjectStructureItem
    {
        public static ProjectStructureItem Build(StartElementEvent saxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            if (string.Equals("Reference", saxEvent.Name))
            {
                var hintPath = descendants?.OfType<EndElementEvent>()
                                          ?.FirstOrDefault(x => string.Equals(x.Name, "HintPath"))
                                          ?.Descendants
                                          ?.OfType<StringElementEvent>()
                                          ?.FirstOrDefault()
                                          ?.Content;
                return new Reference(hintPath);
            }
            return null;
        }

        public static ProjectStructureItem Build(EndElementEvent saxEvent)
        {
            if (string.Equals("Reference", saxEvent.Name))
            {
                var include = saxEvent.Attributes["Include"];
                return new Reference(include);
            }
            return null;
        }
    }

    public class Reference : ProjectStructureItem
    {
        public Reference(string hintPath)
        {
            HintPath = hintPath;
        }

        public string HintPath { get; private set; }
    }

    public abstract class SaxEvent
    {
    }

    public abstract class CompositeSaxEvent : SaxEvent
    {
        public string Uri { get; }

        public string Name { get; }

        public IReadOnlyDictionary<string, string> Attributes { get; }

        public bool IsClosed { get; }

        protected CompositeSaxEvent(string uri, string name, bool isClosed, IReadOnlyDictionary<string, string> attributes)
        {
            Uri = uri;
            Name = name;
            Attributes = attributes;
            IsClosed = isClosed;
        }
    }

    public class StartElementEvent : CompositeSaxEvent
    {
        public StartElementEvent(string uri, string name, bool isClosed, IReadOnlyDictionary<string, string> attributes)
            : base(uri, name, isClosed, attributes)
        {
        }
    }

    public class EndElementEvent : CompositeSaxEvent
    {
        public IReadOnlyCollection<SaxEvent> Descendants { get; }

        public EndElementEvent(string uri, string name, bool isClosed, IReadOnlyDictionary<string, string> attributes, IReadOnlyCollection<SaxEvent> descendants)
            : base(uri, name, isClosed, attributes)
        {
            Descendants = descendants;
        }
    }

    public class StringElementEvent : SaxEvent
    {
        public string Content { get; }

        public StringElementEvent(string content) : base()
        {
            Content = content;
        }
    }

    public class ContentHolder
    {
        public ContentHolder Parent { get; }

        private readonly List<SaxEvent> _content;

        public ContentHolder() : this(null)
        {
        }

        public ContentHolder(ContentHolder parent)
        {
            _content = new List<SaxEvent>();
            Parent = parent;
        }

        public void Append(SaxEvent contentItem)
        {
            _content.Add(contentItem);
        }

        public IReadOnlyCollection<SaxEvent> GetContent()
        {
            return _content.AsReadOnly();
        }
    }

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

    public class SaxParserObserver : IObserver<SaxEvent>
    {
        private readonly IObserver<ProjectStructureItem> _observer;

        private readonly Stack<SaxEvent> _stack;

        public SaxParserObserver(IObserver<ProjectStructureItem> observer)
        {
            _observer = observer;
            _stack = new Stack<SaxEvent>();
        }

        public void OnNext(SaxEvent value)
        {
            var start = value as StartElementEvent;
            if (start != null)
            {
                if (start.IsClosed)
                {
                    Debug.WriteLine($"><{start.Name}");
                    _observer.OnNext(ProjectStructureItem.Build(start, new List<SaxEvent>()));
                    return;
                }

                Debug.WriteLine($"->{start.Name}");
                _stack.Push(value);
                return;
            }

            var end = value as EndElementEvent;
            if (end != null)
            {
                var elementName = end.Name;

                if (end.IsClosed)
                {
                    Debug.WriteLine($"<-{elementName}");
                    _observer.OnNext(ProjectStructureItem.Build(end));
                    return;
                }

                var content = end.Descendants;
                var startCandidate = _stack.Peek() as StartElementEvent;
                if (startCandidate != null)
                {
                    if (string.Equals(startCandidate.Name, elementName))
                    {
                        Debug.WriteLine($"<-{elementName}");
                        _stack.Pop();
                        _observer.OnNext(ProjectStructureItem.Build(startCandidate, content));
                        return;
                    }
                    //_observer.OnError(new ApplicationException($"unexpected closing tag. expected: {startCandidate.Name} actual: {elementName}"));
                }
                //_observer.OnError(new ApplicationException($"unexpected end element: {elementName}"));
            }
        }

        public void OnError(Exception error)
        {
            _observer.OnError(error);
        }

        public void OnCompleted()
        {
            if (_stack.Count == 0)
            {
                _observer.OnCompleted();
            }
            else
            {
                _observer.OnError(new ApplicationException("inconsintent event sequence"));
            }
        }
    }

    public class ProjectFileParser
    {
        public IObservable<ProjectStructureItem> Parse(string fullPath)
        {
            return Observable.Create<ProjectStructureItem>(
                x =>
                    {
                        var saxParser = new SaxParser();
                        var d1 = saxParser.Parse(fullPath, CurrentThreadScheduler.Instance)
                                          .Subscribe(new SaxParserObserver(x));
                        var d2 = CurrentThreadScheduler.Instance.Schedule(x.OnCompleted);
                        return new CompositeDisposable(d1, d2);
                    });
        }
    }
}