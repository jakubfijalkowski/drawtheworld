using Autofac;
using DrawTheWorld.Core.Platform;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace DrawTheWorld.Game
{
	/// <summary>
	/// Main application class.
	/// </summary>
	sealed partial class App
		: Application, IApp
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.Application");

        private readonly IContainer Container = null;
        private readonly ILifetimeScope Scope = null;

        private readonly Platform.LifecycleManager LifecycleManager = null;
		
		/// <summary>
		/// Gets current <see cref="App"/>.
		/// </summary>
		public new static IApp Current
		{
			get { return (Application.Current as IApp) ?? StubApp.Instance; }
		}

		/// <inheritdoc />
		public IUIManager UIManager { get; set; }

		/// <inheritdoc />
		public ISoundManager SoundManager { get; set; }

		/// <inheritdoc />
		public IGameManager GameManager { get; set; }

		/// <inheritdoc />
		public ISettingsProvider Settings { get; set; }

		/// <inheritdoc />
		public UISettings UISettings { get; set; }

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public App()
		{
            this.Container = AutofacConfigurator.Build();

            this.InitializeComponent();

            this.Scope = this.Container.BeginLifetimeScope();
            this.Scope.InjectProperties(this);
            this.LifecycleManager = this.Scope.Resolve<Platform.LifecycleManager>();

            this.Resuming += this.OnResuming;
			this.Suspending += this.OnSuspending;
		}

		protected async override void OnLaunched(LaunchActivatedEventArgs args)
		{
			await this.LifecycleManager.Initialize(args);
		}

		protected override async void OnFileActivated(FileActivatedEventArgs args)
		{
			await this.LifecycleManager.Initialize(args);
			await this.LifecycleManager.OnFileActivated(args);
		}

		private async void OnResuming(object sender, object e)
		{
			await this.LifecycleManager.Resume();
		}

		private async void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			await this.LifecycleManager.Suspend();
			deferral.Complete();
		}
	}
}
