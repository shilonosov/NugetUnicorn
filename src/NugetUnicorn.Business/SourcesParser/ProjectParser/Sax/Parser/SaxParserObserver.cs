using System;
using System.Collections.Generic;

using NugetUnicorn.Business.Extensions;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax.Parser
{
    public class SaxParserObserver<T> : IObserver<SaxEvent>
    {
        private readonly IObserver<T> _observer;

        private readonly IXmlModelBuilder<T> _modelBuilder;

        private readonly Stack<SaxEvent> _stack;

        public SaxParserObserver(IObserver<T> observer, IXmlModelBuilder<T> modelBuilder)
        {
            _observer = observer;
            _modelBuilder = modelBuilder;
            _stack = new Stack<SaxEvent>();
        }

        public void OnNext(SaxEvent value)
        {
            var startElement = value as StartElementEvent;
            if (startElement != null)
            {
                HandleEmptyTag(value, startElement);
                return;
            }

            var endElement = value as EndElementEvent;
            if (endElement != null)
            {
                if (endElement.IsClosed)
                {
                    HandleClosedTag(endElement);
                }
                else
                {
                    HandleCloseTag(endElement);
                }
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

        private void HandleCloseTag(EndElementEvent end)
        {
            var elementName = end.Name;
            var start = _stack.Peek() as StartElementEvent;
            if (start != null)
            {
                if (!string.Equals(start.Name, elementName))
                {
                    _observer.OnError(new ApplicationException($"unexpected closing tag. expected: {start.Name} actual: {elementName}"));
                }
                else
                {
                    _stack.Pop();
                    //TODO: check if end and start are interchangable here
                    _modelBuilder.ComposeElement(start, end.Descendants)
                                 .ForEachItem(_observer.OnNext);
                    return;
                }
            }
            _observer.OnError(new ApplicationException($"unexpected end element: {elementName}"));
        }

        private void HandleClosedTag(EndElementEvent end)
        {
            _modelBuilder.ComposeElement(end)
                         .ForEachItem(_observer.OnNext);
        }

        private void HandleEmptyTag(SaxEvent value, StartElementEvent start)
        {
            _stack.Push(value);
        }
    }
}