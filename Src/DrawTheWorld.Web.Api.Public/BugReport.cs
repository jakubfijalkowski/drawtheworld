namespace DrawTheWorld.Web.Api.Public
{
	/// <summary>
	/// Describes the bug report.
	/// </summary>
	public sealed class BugReport
	{
		/// <summary>
		/// Gets or sets the logs.
		/// </summary>
		public string Logs { get; set; }

		/// <summary>
		/// Gets or sets message that the user provided.
		/// </summary>
		public string Message { get; set; }
	}
}
