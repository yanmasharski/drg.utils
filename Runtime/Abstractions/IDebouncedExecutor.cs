namespace DRG.Utils
{
	using System;
	using System.Collections;

	/// <summary>
	/// Interface for executing actions with a cooldown period between executions.
	/// Provides debouncing functionality to prevent actions from being called too frequently.
	/// </summary>
	public interface IDebouncedExecutor
	{
		ICommand Execute(int framesCooldown, IEnumerator action);
		ICommand Execute(int framesCooldown, Action action);

		public interface ICommand
		{
			bool isRunning { get; }
			void Cancel();
		}
	}
}

