using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

#if UNITY_ANDROID
using Google.Play.Review;
#endif

public static class AppReviewDialog
{
#if UNITY_ANDROID
    private static ReviewManager googleReviewManager;
#endif

    static AppReviewDialog()
    {
#if UNITY_ANDROID
        googleReviewManager = new ReviewManager();
#endif
    }

    public static async UniTask Show(Action<bool> onComplete = null)
    {
#if UNITY_IOS
        if (!UnityEngine.iOS.Device.RequestStoreReview())
        {
            Debug.Log("AppReviewDialog.error: " + "Failed to request store review");
            onComplete?.Invoke(false);
            return;
        }

        onComplete?.Invoke(true);
#endif

#if UNITY_ANDROID
        try
        {
            var requestFlowOperation = googleReviewManager.RequestReviewFlow();
            await requestFlowOperation;

            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                Debug.Log("AppReviewDialog.error: " + requestFlowOperation.Error);
                onComplete?.Invoke(false);
                return;
            }

            var launchFlowOperation = googleReviewManager.LaunchReviewFlow(requestFlowOperation.GetResult());
            await launchFlowOperation;

            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                Debug.Log("AppReviewDialog.error: " + launchFlowOperation.Error);
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log("AppReviewDialog.finished");
            onComplete?.Invoke(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("AppReviewDialog.error: " + e.Message);
            onComplete?.Invoke(false);
        }
#endif
    }
}
