using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using NugetUnicorn.Dto;
using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Utils.Sax;
using NugetUnicorn.Utils.Sax.Parser;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
{
    public class ProjectFileParser
    {
        private readonly SaxParser _saxParser;

        public ProjectFileParser()
        {
            _saxParser = new SaxParser();
        }

        public ProjectPoco Parse(string fullPath)
        {
            var projectStructure = Observable.Create<ProjectStructureItem>(x => Parse(fullPath, x))
                                             .ToEnumerable();

            return new ProjectPoco(fullPath, projectStructure);
        }

        private static IDisposable ParseInternal(IObservable<SaxEvent> observable, IObserver<ProjectStructureItem> observer)
        {
            var saxParserObserver = new SaxParserObserver<ProjectStructureItem>(observer, new ProjectStructureModelBuilder());

            var d1 = observable.Subscribe(saxParserObserver);
            var d2 = CurrentThreadScheduler.Instance
                                           .Schedule(observer.OnCompleted);
            return new CompositeDisposable(d1, d2);
        }

        public IDisposable Parse(string fullPath, IObserver<ProjectStructureItem> x)
        {
            var observable = _saxParser.Parse(fullPath, CurrentThreadScheduler.Instance);
            return ParseInternal(observable, x);
        }

        public IDisposable Parse(TextReader textReader, IObserver<ProjectStructureItem> observer)
        {
            var observable = _saxParser.Parse(textReader, CurrentThreadScheduler.Instance);
            return ParseInternal(observable, observer);
        }
    }
}