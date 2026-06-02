using System;
using DRG.Core.Logs;

namespace DRG.Utils
{
	/// <summary>
	/// Wired by <c>DRG.Utils.Editor</c> at domain load. Runtime proxies use this in the Unity Editor.
	/// </summary>
	public static class AppReviewEditorBridge
	{
		public static Func<ILogger, IAppReviewDialog> CreateAppReviewDialog;
	}
}
