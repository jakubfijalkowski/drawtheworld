using System;
using DrawTheWorld.Core.Online;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// The type of the requirement.
	/// </summary>
	enum RequirementType
		: int
	{
		Action,
		FirstRun,
		FirstRunFullVersion
	}

	/// <summary>
	/// Displays message dialog that requires signing in.
	/// </summary>
	sealed partial class RequireSignIn
		: UserControl, Platform.IMessageDialog
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.RequireLogin");

		private static readonly int[,] ScreenIndexes = new int[,]
		{
			{ 0, 3, 4, 6 },
			{ 1, 3, 5, 7 },
			{ 2, 3, 5, 7 }
		};

		private const int Menu = 0;
		private const int ProcessIndicator = 1;
		private const int SuccessMessage = 2;
		private const int FailureMessage = 3;

		private readonly AccountManager Account = null;

		/// <summary>
		/// Gets the result of the operation.
		/// </summary>
		public bool Result { get; private set; }

		/// <summary>
		/// Gets or sets the type of the requirement.
		/// </summary>
		public RequirementType Requirement { get; set; }

		/// <summary>
		/// Initializes the control.
		/// </summary>
		/// <param name="account"></param>
		public RequireSignIn(AccountManager account)
		{
			this.InitializeComponent();

			this.Account = account;
		}

		/// <inheritdoc />
		public Windows.Foundation.IAsyncAction OpenAsync()
		{
			Logger.Trace("Opening the dialog.");

			this.Result = false;
			this.Popup.SelectedIndex = ScreenIndexes[(int)this.Requirement, Menu];
			return this.Popup.OpenAsync();
		}

		/// <inheritdoc />
		public event EventHandler<object> Closed
		{
			add { this.Popup.Closed += value; }
			remove { this.Popup.Closed -= value; }
		}

		private async void SingIn(object sender, RoutedEventArgs e)
		{
			Logger.Trace("Signing user in.");

			bool failure = false;
			try
			{
				this.Popup.PreventClose = true;
				await this.Popup.SelectIndexAsync(ScreenIndexes[(int)this.Requirement, ProcessIndicator]);
				this.Result = await this.Account.SignIn(false);
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot sign user in.", ex);
				failure = true;
			}
			finally
			{
				this.Popup.PreventClose = false;
			}

			if (!this.Result)
			{
				Logger.Trace("Action aborted.");
				if (failure)
					this.Popup.SelectedIndex = ScreenIndexes[(int)this.Requirement, FailureMessage];
				else
					this.Popup.Close();
			}
			else
			{
				Logger.Info("User signed in.");
				this.Popup.SelectedIndex = ScreenIndexes[(int)this.Requirement, SuccessMessage];
			}
		}

		private void ClosePopup(object sender, RoutedEventArgs e)
		{
			this.Popup.Close();
		}
	}
}
