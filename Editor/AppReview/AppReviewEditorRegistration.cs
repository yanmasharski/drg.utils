using UnityEditor;

namespace DRG.Utils
{
	[InitializeOnLoad]
	internal static class AppReviewEditorRegistration
	{
		static AppReviewEditorRegistration()
		{
			AppReviewEditorBridge.CreateAppReviewDialog = logger => new EditorAppReviewDialog(logger);
		}
	}
}
