using System;
using System.Threading.Tasks;
using DRG.Core.Logs;

namespace DRG.Utils
{
#if UNITY_ANDROID
    using Google.Play.Review;
#endif

	public class AppReviewDialog : IAppReviewDialog
	{
		private readonly ILogger _logger;

#if UNITY_ANDROID
        private ReviewManager reviewManager;
#endif

		public AppReviewDialog(ILogger logger)
		{
			_logger = logger ?? NullLogger.Instance;
		}

		public void Show(Action<bool> onComplete = null)
		{
#if UNITY_IOS
            var success = UnityEngine.iOS.Device.RequestStoreReview();
            if (!success)
            {
                logger.LogWarning("AppReviewDialog: Failed to request store review.");
                onComplete?.Invoke(false);
                return;
            }

            logger.Log("AppReviewDialog: Review requested.");
            onComplete?.Invoke(true);
            return;
#endif

#if UNITY_ANDROID
            StaticMonoBehaviour.instance.StartCoroutine(ShowAndroidCoroutine(onComplete));
            return;
#endif

			_logger.LogWarning("AppReviewDialog: Not supported on this platform.");
			onComplete?.Invoke(false);
		}

		public Task<bool> ShowAsync()
		{
			var tcs = new TaskCompletionSource<bool>();
			Show(result => tcs.TrySetResult(result));
			return tcs.Task;
		}

#if UNITY_ANDROID
        private IEnumerator ShowAndroidCoroutine(Action<bool> onComplete)
        {
            if (reviewManager == null)
            {
                reviewManager = new ReviewManager();
            }

            try
            {
                var requestFlowOperation = reviewManager.RequestReviewFlow();
                yield return requestFlowOperation;

                if (requestFlowOperation.Error != ReviewErrorCode.NoError)
                {
                    logger.LogWarning($"AppReviewDialog: RequestReviewFlow error: {requestFlowOperation.Error}");
                    onComplete?.Invoke(false);
                    yield break;
                }

                var launchFlowOperation = reviewManager.LaunchReviewFlow(requestFlowOperation.GetResult());
                yield return launchFlowOperation;

                if (launchFlowOperation.Error != ReviewErrorCode.NoError)
                {
                    logger.LogWarning($"AppReviewDialog: LaunchReviewFlow error: {launchFlowOperation.Error}");
                    onComplete?.Invoke(false);
                    yield break;
                }

                logger.Log("AppReviewDialog: finished.");
                onComplete?.Invoke(true);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                onComplete?.Invoke(false);
            }
        }
#endif

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

