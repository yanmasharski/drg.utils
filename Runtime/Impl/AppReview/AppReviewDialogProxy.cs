using System;
using System.Threading.Tasks;
using DRG.Core.Logs;

namespace DRG.Utils
{
	/// <summary>
	/// Chooses the best available <see cref="IAppReviewDialog"/> implementation for the current environment.
	/// In the Editor it will try to use an editor-native dialog implementation (from DRG.Utils.Editor assembly),
	/// while in player it falls back to <see cref="AppReviewDialog"/>.
	/// </summary>
	public sealed class AppReviewDialogProxy : IAppReviewDialog
	{
		private readonly IAppReviewDialog _impl;

		public AppReviewDialogProxy(ILogger logger)
		{
			_impl = CreateBest(logger);
		}

		public void Show(Action<bool> onComplete = null) => _impl.Show(onComplete);
		public Task<bool> ShowAsync() => _impl.ShowAsync();

		private static IAppReviewDialog CreateBest(ILogger logger)
		{
#if UNITY_EDITOR
			return AppReviewEditorBridge.CreateAppReviewDialog?.Invoke(logger) ?? new AppReviewDialog(logger);
#else
            return new AppReviewDialog(logger);
#endif
		}
	}
}

