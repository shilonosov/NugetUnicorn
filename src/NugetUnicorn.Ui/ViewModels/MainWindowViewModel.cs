using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using Microsoft.Win32;

using NugetUnicorn.Business.SourcesParser;
using NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Bridge;
using NugetUnicorn.Ui.Controls;
using NugetUnicorn.Ui.Models;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace NugetUnicorn.Ui.ViewModels
{
    public class MainWindowViewModel
    {
        public ReactiveCollection<PackageControlViewModel> Packages { get; private set; }

        public ReactiveProperty<string> SelectedSolutionProperty { get; }

        public ReactiveCommand SelectSolutionCommand { get; }

        public ReactiveProperty<string> ReportString { get; }

        public ReactiveProperty<bool> UiSwitch { get; }

        public MainWindowViewModel(MainWindowModel model)
        {
            Packages = model.PackageKeys
                            .Select(x => new PackageControlViewModel(x))
                            .ToObservable()
                            .ToReactiveCollection();

            UiSwitch = new ReactiveProperty<bool>(true);

            SelectedSolutionProperty = new ReactiveProperty<string>();
            var reactivePropertyObserverBridgeStringReplace = new ReactivePropertyObserverBridgeStringReplace(SelectedSolutionProperty);

            ReportString = new ReactiveProperty<string>(string.Empty);
            var reactivePropertyObserverBridgeStringAdd = new ReactivePropertyObserverBridgeStringAdd(ReportString);

            SelectSolutionCommand = new ReactiveCommand(UiSwitch);
            SelectSolutionCommand.Select(x => SelectSolutionToInspect())
                                 .Where(x => x != null)
                                 .Do(x => UiSwitch.Value = false)
                                 .Do(reactivePropertyObserverBridgeStringReplace)
                                 .Select(x => new SolutionReferenseAnalyzer(new NewThreadScheduler(), x).Subscribe())
                                 .SelectMany(x => x.Finally(() => UiSwitch.Value = true))
                                 .Timestamp()
                                 .Select(x => $"[{x.Timestamp.ToString("s")}] {x.Value}")
                                 .Catch<string, Exception>(x => Observable.Return($"error: {x.Message}"))
                                 .Subscribe(reactivePropertyObserverBridgeStringAdd);
        }

        private string SelectSolutionToInspect()
        {
            var openFileDialog = new OpenFileDialog
                                     {
                                         InitialDirectory = @"c:\Projects",
                                         Filter = "Solution files (*.sln)|*.sln|All files (*.*)|*.*",
                                         FilterIndex = 1,
                                         RestoreDirectory = true
                                     };
            var showDialogResult = openFileDialog.ShowDialog();
            if (!showDialogResult.HasValue || !showDialogResult.Value)
            {
                return null;
            }

            return openFileDialog.FileName;
        }
    }
}