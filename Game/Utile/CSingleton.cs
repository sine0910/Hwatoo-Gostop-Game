using UnityEngine;
using System.Collections;

public abstract class CSingletonMonobehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	static T Instance;

	public static T instance
	{
		get
		{
			if (null == Instance)
			{
				Instance = FindObjectOfType(typeof(T)) as T;
				if (null == Instance)
				{
                    return null;
                }
			}
			return Instance;
		}
	}
}


public abstract class CSingleton<T> where T : class, new()
{
	static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new T();
			}

			return instance;
		}
	}
}
