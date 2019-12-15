using System;
using System.Threading.Tasks;
using DrawTheWorld.Web.Api.Public;
using FLib;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Provides ability to display bug reporting UI.
	/// </summary>
	sealed partial class BugReporting
		: UserControl
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.BugReporting");

		private const int ReportingMenu = 0;
		private const int SendingIndicator = 1;
		private const int SuccessMessage = 2;
		private const int FailureMessage = 3;

		private const int FinishMessageTime = 3000;
		private readonly Client Client = null;

		/// <summary>
		/// Initializes the control.
		/// </summary>
		public BugReporting(Client client)
		{
			this.Client = client;

			this.InitializeComponent();
		}

		private async void OpenPopup(Windows.UI.Popups.IUICommand c = null)
		{
			Logger.Trace("Opening bug reporting UI.");
			if (this.UserMessage != null)
				this.UserMessage.Text = string.Empty;

			this.Popup.SelectedIndex = ReportingMenu;
			await this.Popup.OpenAsync();

			// MessageDialog fix
			if (this.UserMessage == null)
				this.UserMessage = this.Popup.Items[ReportingMenu].FindName(this.NameOf(_ => _.UserMessage)) as TextBox;
		}

		private void ClosePopup(object sender, RoutedEventArgs e)
		{
			this.Popup.Close();
		}

		private async void SendReport(object sender, RoutedEventArgs e)
		{
			bool isError = false;
			try
			{
				this.Popup.PreventClose = true;
				Logger.Info("Sending bug report.");
				await this.Popup.SelectIndexAsync(SendingIndicator);

				var report = new BugReport
				{
					Logs = string.Empty, // await Utilities.BugReportFileTarget.GetBugReportContent(),
					Message = this.UserMessage.Text
				};
				Logger.Info("Logs: " + report.Logs);
				Logger.Info("Message: " + report.Message);

				//var result = await this.Client.PostAsync("bugs/report", report, this.Client.User != null);
				//if (!result.IsSuccessStatusCode)
				//{
				//	Logger.Error("Cannot send bug report. Error code: {0}, phrase: {1}.", result.StatusCode, result.ReasonPhrase);
				//	isError = true;
				//}
				//else
				//{
				//	Logger.Info("Bug report sent.");
				//	isError = false;
				//}
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot send bug report - exception occured. Aborting.", ex);
				isError = true;
			}
			finally
			{
				this.Popup.PreventClose = false;
			}
			await this.Popup.SelectIndexAsync(isError ? FailureMessage : SuccessMessage);
		}
	}
}
