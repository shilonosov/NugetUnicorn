using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace NugetUnicorn.Business.Extensions
{
    public static class ObservableExtensions
    {
        public enum CutterAction
        {
            Continue,

            Skip,

            Break
        }

        public static IObservable<IList<T>> Cutted<T>(this IObservable<T> observable, Func<T, CutterAction> controlFunc)
        {
            return Observable.Create<IList<T>>(
                o =>
                    {
                        IList<T> thisCut = null;
                        return observable.Subscribe(
                            x =>
                                {
                                    var control = controlFunc(x);
                                    switch (control)
                                    {
                                        case CutterAction.Continue:
                                            {
                                                thisCut = thisCut ?? new List<T>();
                                                thisCut.Add(x);
                                                break;
                                            }
                                        case CutterAction.Break:
                                            {
                                                if (thisCut != null)
                                                {
                                                    o.OnNext(thisCut);
                                                    thisCut = null;
                                                }
                                                o.OnCompleted();
                                                break;
                                            }
                                        case CutterAction.Skip:
                                        default:
                                            {
                                                if (thisCut != null)
                                                {
                                                    o.OnNext(thisCut);
                                                }
                                                thisCut = new List<T>();
                                                thisCut.Add(x);
                                                break;
                                            }
                                    }
                                },
                            o.OnError,
                            () =>
                                {
                                    if (thisCut != null)
                                    {
                                        o.OnNext(thisCut);
                                    }
                                    o.OnCompleted();
                                });
                    });
        }
    }
}