namespace DRG.Utils
{
    using UnityEngine;

    /// <summary>
    /// Static MonoBehaviour that ensures a single instance of the class is created and persists across scenes.
    /// </summary>
    public class StaticMonoBehaviour : MonoBehaviour
    {
        private static StaticMonoBehaviour instanceCache;
        public static StaticMonoBehaviour instance
        {
            get
            {
                if (instanceCache == null)
                {
                    instanceCache = new GameObject("StaticMonoBehaviour").AddComponent<StaticMonoBehaviour>();
                    GameObject.DontDestroyOnLoad(instanceCache.gameObject);
                }
                return instanceCache;
            }
        }
    }
}
