using System;
using DRG.Core;

namespace DRG.Utils
{
	/// <summary>
	/// Bridges IMainThreadDispatcher (pure C# interface) to Unity's MainThreadDispatcher.
	/// Register an instance of this class in the root ServiceLocator at startup.
	/// </summary>
	public class MainThreadDispatcherAdapter : IMainThreadDispatcher
	{
		public void Dispatch(Action action) => MainThreadDispatcher.Enqueue(action);
	}
}
