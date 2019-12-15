using Autofac;
using DrawTheWorld.Core.Online;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Core.UserData;
using DrawTheWorld.Core.UserData.Repositories;
using DrawTheWorld.Game.Platform;
using DrawTheWorld.Game.Platform.Stores;

namespace DrawTheWorld.Game
{
    static class AutofacConfigurator
    {
        public static IContainer Build()
        {
            ContainerBuilder builder = new ContainerBuilder();

            RegisterOther(builder);
            RegisterManagers(builder);
            RegisterStores(builder);
            RegisterRepositories(builder);
            RegisterProviders(builder);
            RegisterAccountProviders(builder);

            RegisterPages(builder);

            return builder.Build();
        }

        private static void RegisterOther(ContainerBuilder builder)
        {
            builder.Register(c => new Web.Api.Public.Client(Core.Config.WebApiUrl)).AsSelf().SingleInstance();

            builder.RegisterType<DataInitializer>().AsSelf().SingleInstance();
            builder.RegisterType<PackList>().AsSelf();

            builder.RegisterType<Controls.SettingsAndAccount>().AsSelf();
            builder.RegisterType<Controls.CoinsUI>().AsSelf();
            builder.RegisterType<Controls.ModeSelector>().AsSelf();
            builder.RegisterType<Controls.AppInfo>().AsSelf();
            builder.RegisterType<Controls.BugReporting>().AsSelf();
            builder.RegisterType<Controls.RequireSignIn>().AsSelf();
            builder.RegisterType<Utilities.UIContainer>().AsSelf();

            builder.RegisterType<Utilities.CustomResourceLoader>().AsSelf()
                .AutoActivate()
                .OnActivated(e => Windows.UI.Xaml.Resources.CustomXamlResourceLoader.Current = e.Instance);

            builder.RegisterType<Utilities.Versioning.ApplicationDataVersioner>().AsSelf();
            builder.RegisterType<UISettings>().AsSelf().SingleInstance();

            builder.RegisterType<Utilities.GlobalSettingsManager>().AsSelf().SingleInstance();
        }

        private static void RegisterManagers(ContainerBuilder builder)
        {
            builder.RegisterType<UserStatistics>().AsSelf().SingleInstance();
            builder.RegisterType<AccountManager>().AsSelf().SingleInstance();
            builder.RegisterType<UserAccountManager>().AsSelf().SingleInstance();
            builder.RegisterType<DesignerDataManager>().AsSelf().SingleInstance();
            builder.RegisterType<GameDataManager>().AsSelf().SingleInstance();

            builder.RegisterType<Platform.Notifications>().As<Core.UI.Notifications>().AsSelf().SingleInstance();
        }

        private static void RegisterStores(ContainerBuilder builder)
        {
            builder.Register(c => new UnencryptedPackStore(StoreType.Custom, c.Resolve<IFeatureProvider>())).As<ICustomPackStore>();
            builder.Register(c => new UnencryptedPackStore(StoreType.Designer, c.Resolve<IFeatureProvider>())).As<IDesignerStore>();
            builder.Register(c => new UnencryptedPackStore(StoreType.Demo, c.Resolve<IFeatureProvider>())).As<IDemoPackStore>();
            builder.RegisterType<EncryptedPackStore>().As<IUserPackStore>();

            builder.RegisterType<DesignerThumbnailStore>().As<IThumbnailStore<MutableBoardData>>();
            builder.RegisterType<GameThumbnailStore>().As<IThumbnailStore<GameBoard>>();
            builder.RegisterType<StatisticsStore>().As<IStatisticsStore>();
            builder.RegisterType<LinkedPackStore>().As<ILinkedPackStore>();
        }

        private static void RegisterRepositories(ContainerBuilder builder)
        {
            builder.RegisterType<DesignerRepository>().AsSelf().SingleInstance();
            builder.RegisterType<CustomPackRepository>().AsSelf().SingleInstance();
            builder.RegisterType<OnlineRepository>().AsSelf().SingleInstance();
            builder.RegisterType<DemoRepository>().AsSelf().SingleInstance();
            builder.RegisterType<LinkedPackRepository>().AsSelf().SingleInstance();

            builder.Register(c => c.Resolve<CustomPackRepository>().Provider).As<IPackProvider>();
            builder.Register(c => c.Resolve<OnlineRepository>().Provider).As<IPackProvider>();
            builder.Register(c => c.Resolve<DemoRepository>().Provider).As<IPackProvider>();
            builder.Register(c => c.Resolve<LinkedPackRepository>().Provider).As<IPackProvider>();
        }

        private static void RegisterProviders(ContainerBuilder builder)
        {
            builder.RegisterType<FeatureProvider>().AsSelf().As<IFeatureProvider>().SingleInstance();
            builder.RegisterType<PartsProvider>().As<IPartsProvider>().SingleInstance();
            builder.RegisterType<SettingsProvider>().As<ISettingsProvider>().SingleInstance();

            builder.RegisterType<SoundManager>().AsSelf().As<ISoundManager>().SingleInstance();
            builder.RegisterType<UIManager>().AsSelf().As<IUIManager>().SingleInstance();
            builder.RegisterType<GameManager>().AsSelf().As<IGameManager>().SingleInstance();
            builder.RegisterType<LifecycleManager>().AsSelf().As<ILifecycleManager>().SingleInstance();
        }

        private static void RegisterAccountProviders(ContainerBuilder builder)
        {
            builder.RegisterType<LiveAccountProvider>().As<IAccountProvider>();
        }

        private static void RegisterPages(ContainerBuilder builder)
        {
            builder.RegisterType<Pages.DesignerList>().AsSelf();
            builder.RegisterType<Pages.DesignerPage>().AsSelf();
            builder.RegisterType<Pages.GameList>().AsSelf();
            builder.RegisterType<Pages.GamePage>().AsSelf();
            builder.RegisterType<Pages.StorePage>().AsSelf();
            builder.RegisterType<Pages.MainMenu>().AsSelf();
            builder.RegisterType<Pages.TutorialPage>().AsSelf();
        }
    }
}
