using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

using NugetUnicorn.Business.SourcesParser.ProjectParser.Sax.Parser;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
{
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