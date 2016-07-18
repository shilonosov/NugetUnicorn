using System;
using System.Linq;

namespace NugetUnicorn.Business.SourcesParser
{
    public static class Message
    {
        public static void OnNextInfo(this IObserver<Info> observable, string message)
        {
            observable.OnNext(new Info(message));
        }

        public static void OnNextWarning(this IObserver<Info> observable, string message)
        {
            observable.OnNext(new Warning(message));
        }

        public static void OnNextError(this IObserver<Info> observable, string message)
        {
            observable.OnNext(new Error(message));
        }

        public static void OnNextFatal(this IObserver<Info> observable, string message)
        {
            observable.OnNext(new Fatal(message));
        }

        public static Type TypeFromName(string className)
        {
            var infoType = typeof(Info);
            return infoType.Assembly
                           .GetTypes()
                           .Where(y => y.IsSubclassOf(infoType) || y == infoType)
                           .FirstOrDefault(x => string.Equals(x.Name, className)) ?? typeof(Error);
        }

        public class Info
        {
            public string Message { get; protected set; }

            public Info(string message)
            {
                Message = message;
            }

            public override string ToString()
            {
                return $"{GetType().Name}: {Message}";
            }
        }

        public class Warning : Info
        {
            public Warning(string message)
                : base(message)
            {
            }
        }

        public class Error : Warning
        {
            public Error(string message)
                : base(message)
            {
            }
        }

        public class Fatal : Error
        {
            public Fatal(string message)
                : base(message)
            {
            }
        }
    }
}