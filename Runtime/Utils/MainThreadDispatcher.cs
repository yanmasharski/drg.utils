namespace DRG.Utils
{
    using System;
    using System.Collections.Concurrent;
    using UnityEngine;

    /// <summary>
    /// Provides a mechanism to execute code on Unity's main thread from any thread.
    /// This is essential for operations that must interact with Unity's API, which is not thread-safe.
    /// </summary>
    public static class MainThreadDispatcher
    {
        private static readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
        private static MainThreadDispatcherBehaviour instance;

        static MainThreadDispatcher()
        {
            instance = StaticMonoBehaviour.instance.gameObject.AddComponent<MainThreadDispatcherBehaviour>();
        }

        /// <summary>
        /// Queues an action to be executed on the main thread. Thread-safe.
        /// </summary>
        public static void Enqueue(Action action)
        {
            if (action == null)
            {
                return;
            }
            actions.Enqueue(action);
        }

        private class MainThreadDispatcherBehaviour : MonoBehaviour
        {
            private void Update() => ProcessActions();
            private void LateUpdate() => ProcessActions();
            private void FixedUpdate() => ProcessActions();

            private void ProcessActions()
            {
                while (actions.TryDequeue(out var action))
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
