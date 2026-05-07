using UnityEngine;

namespace DRG.Utils
{
	/// <summary>
	/// Static MonoBehaviour that ensures a single instance of the class is created and persists across scenes.
	/// </summary>
	public class StaticMonoBehaviour : MonoBehaviour
	{
		private static StaticMonoBehaviour _instanceCache;
		public static StaticMonoBehaviour instance
		{
			get
			{
				if (_instanceCache == null)
				{
					_instanceCache = new GameObject("StaticMonoBehaviour").AddComponent<StaticMonoBehaviour>();
					GameObject.DontDestroyOnLoad(_instanceCache.gameObject);
				}
				return _instanceCache;
			}
		}
	}
}

