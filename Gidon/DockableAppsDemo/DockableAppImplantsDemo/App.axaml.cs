using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NP.Avalonia.Gidon;
using NP.DependencyInjection.Interfaces;
using NP.Gidon.Messages;
using NP.Grpc.CommonRelayInterfaces;
using NP.IoCy;
using NP.Protobuf;
using System;

namespace DockableAppImplantsDemo
{
    public class App : Application
    {
        internal static IDependencyInjectionContainer<Enum> IoCContainer { get; }

        private static IRelayServer TheRelayServer { get; }

        public static IRelayClient TheRelayClient { get; }

        public static WindowHandleMatcher TheWindowHandleMatcher { get; }

        static App()
        {
            IContainerBuilderWithMultiCells<Enum> containerBuilder = 
                new ContainerBuilder<Enum>();
            containerBuilder.RegisterMultiCell(typeof(Enum), IoCKeys.Topics);
            containerBuilder.RegisterType<IGrpcConfig, GrpcConfig>();
            containerBuilder.RegisterAttributedStaticFactoryMethodsFromClass(typeof(MessagesTopicsGetter));
            containerBuilder.RegisterPluginsFromSubFolders("Plugins/Services");
            IoCContainer = containerBuilder.Build();

            TheRelayServer = IoCContainer.Resolve<IRelayServer>();

            TheRelayClient = IoCContainer.Resolve<IRelayClient>();

            TheWindowHandleMatcher = new WindowHandleMatcher(TheRelayClient);
        }


        public override void Initialize()
        {                
            AvaloniaXamlLoader.Load(this);
        }


        public override void OnFrameworkInitializationCompleted()
        {  
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
                desktop.ShutdownRequested += Desktop_ShutdownRequested;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            TheRelayServer.Shutdown();
        }
    }
}
