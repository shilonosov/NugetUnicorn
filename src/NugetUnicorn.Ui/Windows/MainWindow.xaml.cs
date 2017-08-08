using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

using GraphX.Controls;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;

using NugetUnicorn.Business;
using NugetUnicorn.Business.Dto;
using NugetUnicorn.Ui.Business;
using NugetUnicorn.Ui.Models;
using NugetUnicorn.Ui.ViewModels;
using NugetUnicorn.Utils.Extensions;
using NuGet;

namespace NugetUnicorn.Ui.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ZoomControl.SetViewFinderVisibility(GgZoomctrl, Visibility.Visible);
            //Set Fill zooming strategy so whole graph will be always visible
            GgZoomctrl.ZoomToFill();

            var packageKeys = new PackageKey[]
                                  {
                                      //new PackageKey("NServiceBus.NHibernate", "4.5.5")
                                      //new PackageKey("FluentNHibernate", "1.4.0"),
                                      //new PackageKey("Common.Logging", "3.3.1"),
                                      ////new PackageKey("Common.Logging", "3.3.1"),
                                      //new PackageKey("Common.Logging.Core", "3.3.1"),
                                      //new PackageKey("Iesi.Collections", "3.2.0.4000"),
                                      //new PackageKey("log4net", "1.1.10"),
                                      //new PackageKey("Microsoft.AspNet.WebApi.Client", "5.2.3"),
                                      //new PackageKey("Microsoft.AspNet.WebApi.Core", "5.2.3"),
                                      //new PackageKey("Microsoft.AspNet.WebApi.Owin", "5.2.3"),
                                      //new PackageKey("Microsoft.Owin", "2.0.2"),
                                      //new PackageKey("Microsoft.Owin.Diagnostics", "2.0.2"),
                                      //new PackageKey("Microsoft.Owin.Host.HttpListener", "2.0.2"),
                                      //new PackageKey("Microsoft.Owin.Hosting", "2.0.2"),
                                      //new PackageKey("Microsoft.Owin.SelfHost", "2.0.2"),
                                      //new PackageKey("Newtonsoft.Json", "6.0.8"),
                                      //new PackageKey("NHibernate", "3.4.1.4000"),
                                      //new PackageKey("NServiceBus", "2.6.0.1505"),
                                      //new PackageKey("NServiceBus.StructureMap", "2.6.0.1505"),
                                      //new PackageKey("NUnit", "2.5.7.10213"),
                                      //new PackageKey("Owin", "1.0"),
                                      //new PackageKey("Quartz", "1.0.3"),
                                      //new PackageKey("RhinoMocks", "3.6"),
                                      //new PackageKey("SharpZipLib", "0.86.0"),
                                      //new PackageKey("SmartThreadPool.dll", "2.2.3"),
                                      //new PackageKey("Spring.Aop", "2.0.1"),
                                      //new PackageKey("Spring.Core", "2.0.1"),
                                      //new PackageKey("Spring.Data", "2.0.1"),
                                      //new PackageKey("Spring.Data.NHibernate3", "2.0.1"),
                                      //new PackageKey("Spring.Services", "2.0.1"),
                                      //new PackageKey("Spring.Testing.NUnit", "2.0.1"),
                                      //new PackageKey("structuremap", "2.6.2")
                                  };

            var storage = new Storage<PackageDto>("Storage");
            var packageRepository = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
            var nugetLibraryProxy = new NugetLibraryProxy(storage, packageRepository);
            var inspector = new PackageInspector(nugetLibraryProxy);

            var node = inspector.InspectPackage(packageKeys)
                                .ToList();


            DataContext = new MainWindowViewModel(new MainWindowModel(nugetLibraryProxy, packageKeys));

            var restrictions = new Dictionary<string, VersionSpec>();
            node.ForEachItem(
                x =>
                    {
                        x.Value.Dependencies.ForEachItem(
                            y =>
                                {
                                    var key = y.Id;
                                    VersionSpec restrictedVersion;
                                    if (restrictions.ContainsKey(key))
                                    {
                                        restrictedVersion = y.VersionSpec.Intersect(restrictions[key]);
                                        Debug.WriteLine($"intersecting {y.VersionSpec} with {restrictions[key]} -> {restrictedVersion}");
                                    }
                                    else
                                    {
                                        restrictedVersion = y.VersionSpec;
                                    }
                                    restrictions[key] = restrictedVersion;
                                });
                    });

            restrictions.ForEachItem(
                x => { Debug.WriteLine($"{x.Key} : {x.Value}"); });

            node = node.Select(
                           x => x.Filter<PackageNode>(
                               y =>
                                   {
                                       var key = y.Key.Id;
                                       if (!restrictions.ContainsKey(key))
                                       {
                                           if (x.Parent == null)
                                           {
                                               return true;
                                           }

                                           Debug.WriteLine($"no restriction for {key}");
                                           return false;
                                       }
                                       var restriction = restrictions[key];
                                       return restriction.Satisfies(y.SemanticVersion);
                                   }))
                       .Where(x => x != null)
                       .ToList();

            var graph = new GraphExample();

            var graphNodes = new Dictionary<string, DataVertex>();
            var groupIds = new Dictionary<string, int>();
            var nextId = 1L;
            var nextGroupId = 1;
            var visitedNodes = new HashSet<PackageKey>();

            node.ForEachItem(
                x =>
                    {
                        x.ForEachItem(
                            y =>
                                {
                                    var packageDto = y.Value;
                                    var packageKey = packageDto.Key;
                                    var packageId = packageKey.Id;

                                    var nodeKey = ComposeNodeKey(packageDto);

                                    if (!groupIds.ContainsKey(packageId))
                                    {
                                        groupIds[packageId] = nextGroupId++;
                                    }

                                    if (visitedNodes.Contains(packageKey))
                                    {
                                        return;
                                    }
                                    var existing = storage.GetById(packageId)
                                                          .Value
                                                          .Select(z => z.Key)
                                                          .ToList();
                                    visitedNodes.Add(packageKey);

                                    if (graphNodes.ContainsKey(nodeKey))
                                    {
                                        var existring = graphNodes[nodeKey];
                                        existring.AddVersion(existing, packageKey);
                                        return;
                                    }
                                    var dataVertex = new DataVertex(packageId)
                                                         {
                                                             ID = nextId++,
                                                             GroupId = groupIds[packageId]
                                                         };
                                    dataVertex.AddVersion(existing, packageKey);

                                    graphNodes[nodeKey] = dataVertex;
                                    graph.AddVertex(dataVertex);
                                });
                    });

            node.ForEachItem(
                x =>
                    {
                        x.ForEachItem(
                            y =>
                                {
                                    if (y.Parent == null)
                                    {
                                        return;
                                    }
                                    var packageDto = y.Value;
                                    var thisKey = ComposeNodeKey(packageDto);
                                    var thisVertex = graphNodes[thisKey];

                                    var parent = y.Parent.Value;
                                    var parentKey = ComposeNodeKey(parent);

                                    if (graphNodes.ContainsKey(parentKey))
                                    {
                                        var parentVertex = graphNodes[parentKey];
                                        graph.AddEdge(new DataEdge(thisVertex, parentVertex));
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"no parent node for this: {thisKey} parent: {parentKey}");
                                    }
                                });
                    });

            var logicCore = new GxLogicCoreExample
                                {
                                    Graph = graph
                                };

            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.KK);
            ((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 4000;
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 200;
            logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 150;

            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;

            logicCore.AsyncAlgorithmCompute = false;

            GgArea.LogicCore = logicCore;

            GgArea.GenerateGraph(true, true);

            /* 
             * After graph generation is finished you can apply some additional settings for newly created visual vertex and edge controls
             * (VertexControl and EdgeControl classes).
             * 
             */

            //This method sets the dash style for edges. It is applied to all edges in Area.EdgesList. You can also set dash property for
            //each edge individually using EdgeControl.DashStyle property.
            //For ex.: Area.EdgesList[0].DashStyle = GraphX.EdgeDashStyle.Dash;
            GgArea.SetEdgesDashStyle(EdgeDashStyle.Dash);

            //This method sets edges arrows visibility. It is also applied to all edges in Area.EdgesList. You can also set property for
            //each edge individually using property, for ex: Area.EdgesList[0].ShowArrows = true;
            GgArea.ShowAllEdgesArrows(true);

            //This method sets edges labels visibility. It is also applied to all edges in Area.EdgesList. You can also set property for
            //each edge individually using property, for ex: Area.EdgesList[0].ShowLabel = true;
            GgArea.ShowAllEdgesLabels(true);

            GgZoomctrl.ZoomToFill();
        }

        private static string ComposeNodeKey(PackageDto packageDto)
        {
            return packageDto.Key.Id + "|" + string.Join("|", packageDto.Dependencies.Select(z => z.Id));
        }
    }
}