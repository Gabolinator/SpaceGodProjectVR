using UnityEngine;

namespace Utils
{
	public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected static T _instance;

		public static T Instance
		{
			get
			{
				if (_instance != null) 
					return _instance;
                
				_instance = FindObjectOfType<T>();
				if (_instance != null) 
					return _instance;
                
				var singleton = new GameObject($"{typeof(T).Name} ({nameof(SingletonMonoBehaviour<T>)}");
				_instance = singleton.AddComponent<T>();
				DontDestroyOnLoad(singleton);
				Debug.Log($"Created a new {typeof(T).Name} ({nameof(SingletonMonoBehaviour<T>)}).");
				return _instance;
			}
		}
	}
}