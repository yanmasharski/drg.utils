using System;
using System.Threading.Tasks;
using DRG.Core.Logs;

#if UNITY_EDITOR
namespace DRG.Utils
{
	/// <summary>
	/// Editor-only implementation of <see cref="DRG.Utils.IAppReviewDialog"/>.
	/// Shows a native Unity Editor dialog and returns whether the user confirmed.
	/// </summary>
	public sealed class EditorAppReviewDialog : IAppReviewDialog
	{
		private readonly ILogger _logger;

		public EditorAppReviewDialog(ILogger logger)
		{
			_logger = logger ?? NullLogger.Instance;
		}

		public void Show(Action<bool> onComplete = null)
		{
			EditorNativeDialog.ChooseWithDelayedPrimary(
				title: "App Review",
				message: "In-app review is not available in the Unity Editor.\nSimulate a successful review request?",
				primaryDelaySeconds: 0,
				primary: "Simulate success",
				onPrimary: () =>
				{
					_logger.Log("EditorAppReviewDialog: simulated success.");
					onComplete?.Invoke(true);
				},
				secondary: "Cancel",
				onSecondary: () =>
				{
					_logger.Log("EditorAppReviewDialog: cancelled.");
					onComplete?.Invoke(false);
				},
				tertiary: "Cancel",
				onTertiary: () =>
				{
					_logger.Log("EditorAppReviewDialog: cancelled.");
					onComplete?.Invoke(false);
				});
		}

		public Task<bool> ShowAsync()
		{
			var tcs = new TaskCompletionSource<bool>();
			Show(result => tcs.TrySetResult(result));
			return tcs.Task;
		}

		private sealed class NullLogger : ILogger
		{
			public static readonly NullLogger Instance = new();

			public void Log(string message) { }
			public void LogWarning(string message) { }
			public void LogError(string message) { }
			public void LogException(Exception exception) { }
		}
	}
}
#endif
