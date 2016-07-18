using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

using CommandLine;
using CommandLine.Text;

using NugetUnicorn.Business.SourcesParser;

namespace NUgetUnicorn.Console
{
    internal class Options
    {
        [Option('l', "log-level", DefaultValue = "Error", HelpText = "this is a log level")]
        public string LogLevelString { get; set; }

        [Option('s', "solution-path", DefaultValue = "", HelpText = "this is a solution full path", Required = true)]
        public string SolutionPath { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("NugetUnicorn 1.0");
            usage.AppendLine("You can always read the code (hehehe) on a github: https://github.com/shilonosov/NugetUnicorn");
            usage.Append(HelpText.AutoBuild(this));
            return usage.ToString();
        }
    }

    internal class Program
    {
        public static int Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                return RunAnalyzer(options);
            }
            return -1;
        }

        private static int RunAnalyzer(Options options)
        {
            var filterType = Message.TypeFromName(options.LogLevelString);
            var commonPart = new SolutionReferenseAnalyzer(Scheduler.CurrentThread, options.SolutionPath).Run()
                                                                                                         .Catch<Message.Info, Exception>(
                                                                                                             y => Observable.Return(new Message.Fatal($"error: {y.Message}")))
                                                                                                             .Timestamp()
                                                                                                         .ToEnumerable()
                                                                                                         .ToArray();
            var itemsToPrint = commonPart.Where(x =>
                {
                    var itemType = x.Value.GetType();
                    return itemType.IsSubclassOf(filterType) || itemType == filterType;
                });

            foreach (var outputItem in itemsToPrint)
            {
                System.Console.WriteLine($"{outputItem.Timestamp} {outputItem.Value}");
            }

            return commonPart.Count(x => x.Value.GetType().IsSubclassOf(typeof(Message.Warning)));
        }
    }
}