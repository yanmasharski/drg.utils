using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILogger = DRG.Core.Logs.ILogger;

namespace DRG.Utils
{
	/// <summary>
	/// Internal MonoBehaviour that handles delayed save operations.
	/// TODO Rebuild this completely.
	/// </summary>
	[Obsolete("This class is obsolete and won't be used anymore soon.")]
	public class DebouncedExecutorUnity : IDebouncedExecutor
	{
		private readonly MonoBehaviour _monoBehaviour;
		private readonly Executor _executor;
		private readonly ILogger _logger;

		public DebouncedExecutorUnity(MonoBehaviour monoBehaviour, ILogger logger)
		{
			_monoBehaviour = monoBehaviour;
			_logger = logger;
			_executor = monoBehaviour.gameObject.TryGetComponent<Executor>(out var component) ? component : monoBehaviour.gameObject.AddComponent<Executor>();
		}

		public IDebouncedExecutor.ICommand Execute(int cooldown, IEnumerator action)
		{
			var command = new Command(_monoBehaviour, _executor, _logger);
			command.Execute(cooldown, action);
			return command;
		}

		public IDebouncedExecutor.ICommand Execute(int cooldown, Action action)
		{
			var command = new Command(_monoBehaviour, _executor, _logger);
			command.Execute(cooldown, action);
			return command;
		}

		private class Command : IDebouncedExecutor, IDebouncedExecutor.ICommand
		{
			private readonly MonoBehaviour _monoBehaviour;
			private readonly ILogger _logger;
			private readonly Executor _executor;
			private Coroutine _executionCoroutine;
			private int _framesCooldown;

			private Action _action;

			public Command(MonoBehaviour monoBehaviour, Executor executor, ILogger logger)
			{
				_monoBehaviour = monoBehaviour;
				_logger = logger;
				_executor = executor;
			}

			public bool isRunning => _executionCoroutine != null;

			public void Cancel()
			{
				if (_executionCoroutine == null)
				{
					return;
				}

				_monoBehaviour.StopCoroutine(_executionCoroutine);
				_executor.RemoveCommand(this);
				_action = null;
				_executionCoroutine = null;
			}

			public IDebouncedExecutor.ICommand Execute(int framesCooldown, IEnumerator action)
			{
				if (_executionCoroutine != null)
				{
					_monoBehaviour.StopCoroutine(_executionCoroutine);
				}

				_executionCoroutine = _monoBehaviour.StartCoroutine(ExecutionCoroutine(framesCooldown, action));
				return this;
			}

			public IDebouncedExecutor.ICommand Execute(int framesCooldown, Action action)
			{
				_executor.RemoveCommand(this);
				_executor.AddCommand(this);
				_action = action;
				return this;
			}

			public bool Tick(int frames, float deltaTime)
			{
				if (_framesCooldown > 0)
				{
					_framesCooldown -= frames;

					if (_framesCooldown <= 0)
					{
						_action?.Invoke();
						_action = null;
						return true;
					}

					return false;
				}

				_action?.Invoke();
				_action = null;
				return true;
			}

			private IEnumerator ExecutionCoroutine(int cooldown, IEnumerator action)
			{
				_framesCooldown = cooldown;

				while (_framesCooldown > 0)
				{
					yield return null;
					_framesCooldown--;
				}

				yield return action;

				_executionCoroutine = null;
			}
		}

		private class Executor : MonoBehaviour
		{
			private readonly List<Command> _commands = new();

			public void AddCommand(Command command)
			{
				_commands.Add(command);
			}

			public void RemoveCommand(Command command)
			{
				_commands.Remove(command);
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
				var i = _commands.Count;
				while (--i > -1)
				{
					if (_commands[i].Tick(frames, deltaTime))
					{
						_commands.RemoveAt(i);
					}
				}
			}
		}
	}
}

