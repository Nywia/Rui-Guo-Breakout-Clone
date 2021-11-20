using UnityEngine;

/// <summary>
///     Inherit from this base class to create a singleton 
///     E.g. public class MyClass : Singleton<MyClass> {}
/// </summary>
/// <typeparam name="T">The class type</typeparam>
public class ScrSingletonBase<T> : MonoBehaviour where T : ScrSingletonBase<T>
{
    private static T Internal_Instance = null;

    public static T Instance
    {
        get
        {
            if (Internal_Instance == null)
            {
                // Check if there exists an instance
                Internal_Instance = FindObjectOfType<T>();

                // Fallback just in case
                if (Internal_Instance == null)
                {
                    // Make new instance
                    Internal_Instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
            }
            return Internal_Instance;
        }
    }
}