namespace DRG.Utils
{
    using System;
    using ILogger = DRG.Logs.ILogger;
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Internal MonoBehaviour that handles delayed save operations.
    /// </summary>
    public class DebouncedExecutorUnity : IDebouncedExecutor
    {
        private readonly MonoBehaviour monoBehaviour;
        private readonly Executor executor;
        private readonly ILogger logger;

        public DebouncedExecutorUnity(MonoBehaviour monoBehaviour, ILogger logger)
        {
            this.monoBehaviour = monoBehaviour;
            this.logger = logger;
            executor = monoBehaviour.gameObject.TryGetComponent<Executor>(out var component) ? component : monoBehaviour.gameObject.AddComponent<Executor>();
        }

        public IDebouncedExecutor.ICommand Execute(int cooldown, IEnumerator action)
        {
            var command = new Command(monoBehaviour, executor, logger);
            command.Execute(cooldown, action);
            return command;
        }

        public IDebouncedExecutor.ICommand Execute(int cooldown, Action action)
        {
            var command = new Command(monoBehaviour, executor, logger);
            command.Execute(cooldown, action);
            return command;
        }

        private class Command : IDebouncedExecutor, IDebouncedExecutor.ICommand
        {
            private readonly MonoBehaviour monoBehaviour;
            private readonly ILogger logger;
            private readonly Executor executor;
            private Coroutine executionCoroutine;
            private int framesCooldown;

            private Action action;

            public Command(MonoBehaviour monoBehaviour, Executor executor, ILogger logger)
            {
                this.monoBehaviour = monoBehaviour;
                this.logger = logger;
                this.executor = executor;
            }

            public bool isRunning => executionCoroutine != null;

            public void Cancel()
            {
                if (executionCoroutine == null)
                {
                    return;
                }

                monoBehaviour.StopCoroutine(executionCoroutine);
                executor.RemoveCommand(this);
                action = null;
                executionCoroutine = null;
            }

            public IDebouncedExecutor.ICommand Execute(int framesCooldown, IEnumerator action)
            {
                if (executionCoroutine != null)
                {
                    monoBehaviour.StopCoroutine(executionCoroutine);
                }

                executionCoroutine = monoBehaviour.StartCoroutine(ExecutionCoroutine(framesCooldown, action));
                return this;
            }

            public IDebouncedExecutor.ICommand Execute(int framesCooldown, Action action)
            {
                executor.RemoveCommand(this);
                executor.AddCommand(this);
                this.action = action;
                return this;
            }

            public bool Tick(int frames, float deltaTime)
            {
                if (framesCooldown > 0)
                {
                    framesCooldown -= frames;

                    if (framesCooldown <= 0)
                    {
                        action?.Invoke();
                        action = null;
                        return true;
                    }

                    return false;
                }

                action?.Invoke();
                action = null;
                return true;
            }

            private IEnumerator ExecutionCoroutine(int cooldown, IEnumerator action)
            {
                framesCooldown = cooldown;

                while (framesCooldown > 0)
                {
                    yield return null;
                    framesCooldown--;
                }

                yield return action;

                executionCoroutine = null;
            }
        }

        private class Executor : MonoBehaviour
        {
            private readonly List<Command> commands = new List<Command>();

            public void AddCommand(Command command)
            {
                commands.Add(command);
            }

            public void RemoveCommand(Command command)
            {
                commands.Remove(command);
            }

            private void Update()
            {
                Tick(1, Time.deltaTime);
            }

            private void LateUpdate()
            {
                Tick(0, 0);
            }

            private void Tick(int frames, float deltaTime)
            {
                var i = commands.Count;
                while (--i > -1)
                {
                    if (commands[i].Tick(frames, deltaTime))
                    {
                        commands.RemoveAt(i);
                    }
                }
            }
        }
    }
}
