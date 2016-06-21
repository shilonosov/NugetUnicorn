using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Xml;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
{
    public class ProjectStructureItem
    {
        public static ProjectStructureItem Build(SaxEvent saxEvent)
        {
            if (string.Equals("Reference", saxEvent.Name))
            {
                return new Reference();
            }
            return null;
        }
    }

    public class Reference : ProjectStructureItem
    {
    }

    public abstract class SaxEvent
    {
        public string Uri { get; }

        public string Name { get; }

        public IReadOnlyDictionary<string, string> Attributes { get; }

        protected SaxEvent(string uri, string name, IReadOnlyDictionary<string, string> attributes)
        {
            Uri = uri;
            Name = name;
            Attributes = attributes;
        }
    }

    public class StartElementEvent : SaxEvent
    {
        public bool IsClosed { get; }

        public StartElementEvent(string uri, string name, bool isClosed, IReadOnlyDictionary<string, string> attributes)
            : base(uri, name, attributes)
        {
            IsClosed = isClosed;
        }
    }

    public class EndElementEvent : SaxEvent
    {
        public EndElementEvent(string uri, string name, IReadOnlyDictionary<string, string> attributes)
            : base(uri, name, attributes)
        {
        }
    }

    public class SaxParser
    {
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
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    {
                        HandleStartElement(reader, x);
                        break;
                    }
                case XmlNodeType.EndElement:
                    {
                        HandleEndElement(reader, x);
                        break;
                    }
            }
        }

        private void HandleEndElement(XmlTextReader reader, IObserver<SaxEvent> observer)
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
            observer.OnNext(new EndElementEvent(strUri, strName, new ReadOnlyDictionary<string, string>(attributes)));
        }

        private static void HandleStartElement(XmlTextReader reader, IObserver<SaxEvent> observer)
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
                    Debug.WriteLine($"><{value.Name}");
                    _observer.OnNext(ProjectStructureItem.Build(start));
                    return;
                }

                Debug.WriteLine($"->{value.Name}");
                _stack.Push(value);
                return;
            }

            var end = value as EndElementEvent;
            if (end != null)
            {
                var startCandidate = _stack.Peek() as StartElementEvent;
                if (startCandidate != null)
                {
                    if (string.Equals(startCandidate.Name, end.Name))
                    {
                        Debug.WriteLine($"<-{value.Name}");
                        _stack.Pop();
                        _observer.OnNext(ProjectStructureItem.Build(startCandidate));
                        return;
                    }
                    _observer.OnError(new ApplicationException($"unexpected closing tag. expected: {startCandidate.Name} actual: {end.Name}"));
                }
                _observer.OnError(new ApplicationException($"unexpected end element: {end.Name}"));
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

        private static void ProcessSaxElement(SaxEvent y, IObserver<ProjectStructureItem> observer)
        {
            Debug.WriteLine($"{y.GetType().Name} {y.Name}");
        }
    }
}