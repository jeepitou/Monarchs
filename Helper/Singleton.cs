using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton pattern class.
/// @Eric tu peux essayer de comprendre si tu veux, mais sa serais plus facile que tu google ou me demande c'est quoi un singleton
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour
    where T : Component
{
    private static T _instance;
    public static T Instance {
        get {
            if (_instance == null) {
                var objs = FindObjectsOfType (typeof(T)) as T[];
                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1) {
                    Debug.LogError ("There is more than one " + typeof(T).Name + " in the scene.");
                }
                if (_instance == null) {
                    GameObject obj = new GameObject ();
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    _instance = obj.AddComponent<T> ();
                }
            }
            return _instance;
        }
    }
}

/// <summary>
/// Singleton pattern class for scriptable object.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
{
    private static T _instance;
    public static T Instance {
        get {
            if (_instance == null) {
                var objs = Resources.LoadAll<T>("") as T[];
                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1) {
                    Debug.LogError ("There is more than one " + typeof(T).Name + " in the scene.");
                }
            }
            return _instance;
        }
    }
}