using UnityEngine;

public static class Utils
{
    // Generic method to load a ScriptableObject by name
    public static T LoadScriptableObject<T>(string resourceName) where T : ScriptableObject
    {
        // Try to load the resource
        T loadedObject = Resources.Load<T>(resourceName);

        if (loadedObject == null)
        {
            Debug.LogError($"Failed to load {typeof(T)} with name '{resourceName}'");
            return null;
        }

        return loadedObject;
    }

    // Alternative method that searches all resources of type T
    public static T LoadScriptableObjectOfType<T>() where T : ScriptableObject
    {
        T[] allObjects = Resources.LoadAll<T>("");

        if (allObjects == null || allObjects.Length == 0)
        {
            Debug.LogError($"No objects of type {typeof(T)} found in Resources");
            return null;
        }

        return allObjects[0]; // Return first found
    }
}
