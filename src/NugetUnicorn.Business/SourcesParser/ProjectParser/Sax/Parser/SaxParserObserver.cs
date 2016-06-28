using System;
using System.Collections.Generic;
using System.Diagnostics;

using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax.Parser
{
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
                    _observer.OnError(new ApplicationException($"unexpected closing tag. expected: {startCandidate.Name} actual: {elementName}"));
                }
                _observer.OnError(new ApplicationException($"unexpected end element: {elementName}"));
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
}