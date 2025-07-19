#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Diagnostics;

public class OpenPersistentDataPath : EditorWindow
{
    [MenuItem("Tools/Open Persistent Data Folder")]
    public static void OpenPersistentDataFolder()
    {
        string path = Application.persistentDataPath;
        
        // Open in system file explorer
        EditorUtility.RevealInFinder(path);
        
        // Alternative (cross-platform)
        // Process.Start(path);
    }
}
#endif