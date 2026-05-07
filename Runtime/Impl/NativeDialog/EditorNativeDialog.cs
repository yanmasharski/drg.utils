using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace DRG.Utils
{
	public static class EditorNativeDialog
	{
		public readonly struct Button
		{
			public readonly string Text;
			public readonly Action Callback;

			public Button(string text, Action callback = null)
			{
				Text = text;
				Callback = callback;
			}
		}

		public static void Alert(string title, string message, string ok = "OK", Action onOk = null)
		{
			Choose(
				title: title,
				message: message,
				buttons: new[] { new Button(ok, onOk) },
				delayedButtonNumber: 0,
				delaySeconds: 0,
				fallbackButtonNumber: 1);
		}

		public static void Confirm(
			string title,
			string message,
			string ok = "Yes",
			Action onOk = null,
			string cancel = "No",
			Action onCancel = null)
		{
			Choose(
				title: title,
				message: message,
				buttons: new[]
				{
					new Button(ok, onOk),
					new Button(cancel, onCancel),
				},
				delayedButtonNumber: 0,
				delaySeconds: 0,
				fallbackButtonNumber: 2);
		}

		public static void Choose(
			string title,
			string message,
			string primary = "Yes",
			Action onPrimary = null,
			string secondary = "No",
			Action onSecondary = null,
			string tertiary = "Cancel",
			Action onTertiary = null)
		{
			Choose(
				title: title,
				message: message,
				buttons: new[]
				{
					new Button(primary, onPrimary),
					new Button(secondary, onSecondary),
					new Button(tertiary, onTertiary),
				},
				delayedButtonNumber: 0,
				delaySeconds: 0,
				fallbackButtonNumber: 3);
		}

		/// <summary>
		/// Shows the same dialog as <see cref="Choose"/>, but delays callback invocation by <paramref name="primaryDelaySeconds"/> seconds.
		/// </summary>
		public static void ChooseWithDelayedPrimary(
			string title,
			string message,
			int primaryDelaySeconds,
			string primary = "Yes",
			Action onPrimary = null,
			string secondary = "No",
			Action onSecondary = null,
			string tertiary = "Cancel",
			Action onTertiary = null)
		{
			Choose(
				title: title,
				message: message,
				buttons: new[]
				{
					new Button(primary, onPrimary),
					new Button(secondary, onSecondary),
					new Button(tertiary, onTertiary),
				},
				delayedButtonNumber: 1,
				delaySeconds: primaryDelaySeconds,
				fallbackButtonNumber: 3);
		}

		/// <summary>
		/// Shows a dialog with N buttons (N >= 1).
		/// </summary>
		public static void Choose(string title, string message, params Button[] buttons)
		{
			Choose(title, message, buttons, delayedButtonNumber: 0, delaySeconds: 0, fallbackButtonNumber: buttons?.Length ?? 0);
		}

		/// <summary>
		/// Shows a dialog with N buttons (N >= 1).
		/// If <paramref name="delayedButtonNumber"/> is set (1..N), the callback for that button fires after
		/// <paramref name="delaySeconds"/> seconds. Use 0 to disable delay.
		/// </summary>
		public static void Choose(
			string title,
			string message,
			Button[] buttons,
			int delayedButtonNumber,
			int delaySeconds,
			int fallbackButtonNumber)
		{
			RunOnMainThread(() =>
			{
				if (buttons == null || buttons.Length <= 0)
				{
					Debug.LogWarning("EditorNativeDialog.Choose called with no buttons; falling back to OK.");
					EditorUtility.DisplayDialog(title, message, "OK");
					return;
				}

				var delayedIndex = delayedButtonNumber == 0 ? -1 : delayedButtonNumber - 1;
				if (delayedIndex < -1 || delayedIndex >= buttons.Length)
					delayedIndex = -1;

				ChoiceDialogWindow.Show(
					title: title,
					message: message,
					buttons: buttons,
					delayedIndex: delayedIndex,
					delaySeconds: delaySeconds,
					fallbackIndex: (fallbackButtonNumber <= 0 ? -1 : fallbackButtonNumber - 1));
			});
		}

		private static void RunOnMainThread(Action action)
		{
			if (action == null)
				return;
			EditorApplication.delayCall += () =>
			{
				try
				{ action(); }
				catch (Exception e) { Debug.LogException(e); }
			};
		}

		private static void InvokeAfterSeconds(int seconds, Action action)
		{
			if (action == null)
				return;
			var delay = Mathf.Max(0, seconds);
			if (delay == 0)
			{
				EditorApplication.delayCall += () => action();
				return;
			}

			var executeAt = EditorApplication.timeSinceStartup + delay;

			void Tick()
			{
				if (EditorApplication.timeSinceStartup < executeAt)
					return;
				EditorApplication.update -= Tick;
				try
				{ action(); }
				catch (Exception e) { Debug.LogException(e); }
			}

			EditorApplication.update += Tick;
		}

		private sealed class ChoiceDialogWindow : EditorWindow
		{
			private const float ButtonHeight = 22f;
			private const float VerticalGap = 10f;

			private string titleText;
			private string messageText;
			private Button[] buttons;
			private int delayedIndex;
			private int delaySeconds;
			private int fallbackIndex;
			private Vector2 scroll;
			private bool clicked;

			public static void Show(string title, string message, Button[] buttons, int delayedIndex, int delaySeconds, int fallbackIndex)
			{
				var w = CreateInstance<ChoiceDialogWindow>();
				w.titleText = title ?? string.Empty;
				w.messageText = message ?? string.Empty;
				w.buttons = buttons ?? Array.Empty<Button>();
				w.delayedIndex = delayedIndex;
				w.delaySeconds = Mathf.Max(0, delaySeconds);
				w.fallbackIndex = fallbackIndex;

				w.minSize = new Vector2(420f, 160f);
				w.maxSize = new Vector2(900f, 700f);
				w.position = GetCenteredPosition(w.minSize);
				w.ShowUtility();
				w.Focus();
			}

			private static Rect GetCenteredPosition(Vector2 size)
			{
				var main = GetMainWindowPositionSafe(size);
				return new Rect(
					main.x + (main.width - size.x) * 0.5f,
					main.y + (main.height - size.y) * 0.5f,
					size.x,
					size.y);
			}

			private static Rect GetMainWindowPositionSafe(Vector2 size)
			{
				var method = typeof(EditorGUIUtility).GetMethod(
					"GetMainWindowPosition",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

				if (method != null && method.ReturnType == typeof(Rect))
				{
					try
					{ return (Rect)method.Invoke(null, null); }
					catch { }
				}

				var width = Mathf.Max(640f, size.x);
				var height = Mathf.Max(480f, size.y);
				return new Rect(
					(Screen.currentResolution.width - width) * 0.5f,
					(Screen.currentResolution.height - height) * 0.5f,
					width,
					height);
			}

			private void OnEnable()
			{
				titleContent = new GUIContent(string.IsNullOrEmpty(titleText) ? "Dialog" : titleText);
				EditorApplication.update += Repaint;
			}

			private void OnDisable()
			{
				EditorApplication.update -= Repaint;
				if (!clicked)
					InvokeByIndex(fallbackIndex);
			}

			private void OnGUI()
			{
				if (!string.IsNullOrEmpty(titleText))
				{
					EditorGUILayout.LabelField(titleText, EditorStyles.boldLabel);
					GUILayout.Space(4f);
				}

				EditorGUILayout.HelpBox(messageText ?? string.Empty, MessageType.None);
				GUILayout.Space(VerticalGap);

				scroll = EditorGUILayout.BeginScrollView(scroll);
				for (var i = 0; i < buttons.Length; i++)
				{
					var b = buttons[i];
					var text = string.IsNullOrWhiteSpace(b.Text) ? $"Button {i + 1}" : b.Text;
					if (i == delayedIndex && delaySeconds > 0)
						text = $"{text} ({delaySeconds}s)";

					if (GUILayout.Button(text, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeight)))
					{
						clicked = true;
						Close();
						InvokeByIndex(i);
						GUIUtility.ExitGUI();
					}
				}
				EditorGUILayout.EndScrollView();
			}

			private void InvokeByIndex(int index)
			{
				if (index < 0 || index >= buttons.Length)
					return;
				var cb = buttons[index].Callback;
				if (index == delayedIndex)
					InvokeAfterSeconds(delaySeconds, cb);
				else
					cb?.Invoke();
			}
		}
	}
}
#endif
