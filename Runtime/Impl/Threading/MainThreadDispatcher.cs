using System;
using System.Collections.Concurrent;
using UnityEngine;
using ILogger = DRG.Core.Logs.ILogger;

namespace DRG.Utils
{
	/// <summary>
	/// Provides a mechanism to execute code on Unity's main thread from any thread.
	/// This is essential for operations that must interact with Unity's API, which is not thread-safe.
	/// </summary>
	public static class MainThreadDispatcher
	{
		private readonly struct WorkItem
		{
			public readonly Action Action;
			public readonly ILogger Logger;

			public WorkItem(Action action, ILogger logger)
			{
				Action = action;
				Logger = logger;
			}
		}

		private static readonly ConcurrentQueue<WorkItem> Actions = new();
		private static readonly MainThreadDispatcherBehaviour Instance;

		static MainThreadDispatcher()
		{
			Instance = StaticMonoBehaviour.instance.gameObject.AddComponent<MainThreadDispatcherBehaviour>();
		}

		/// <summary>
		/// Queues an action to be executed on the main thread. Thread-safe.
		/// </summary>
		public static void Enqueue(Action action)
		{
			Enqueue(action, null);
		}

		/// <summary>
		/// Queues an action to be executed on the main thread. Thread-safe.
		/// Allows passing a logger to report exceptions that happen during execution.
		/// </summary>
		public static void Enqueue(Action action, ILogger logger)
		{
			if (action == null)
			{
				return;
			}

			Actions.Enqueue(new WorkItem(action, logger ?? NullLogger.Instance));
		}

		private class MainThreadDispatcherBehaviour : MonoBehaviour
		{
			private void Update() => ProcessActions();
			private void LateUpdate() => ProcessActions();
			private void FixedUpdate() => ProcessActions();

			private void ProcessActions()
			{
				while (Actions.TryDequeue(out var item))
				{
					try
					{
						item.Action?.Invoke();
					}
					catch (Exception e)
					{
						item.Logger?.LogException(() => e);
					}
				}
			}
		}

		private sealed class NullLogger : ILogger
		{
			public static readonly NullLogger Instance = new();

			public void Log(Func<string> message) { }

			public void LogWarning(Func<string> message) { }

			public void LogError(Func<string> message) { }

			public void LogException(Func<Exception> exception) { }
		}
	}
}

