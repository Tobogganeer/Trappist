using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class EditorUtilities
{
    public static List<T> FindAllScriptableObjectsOfType<T>(string typeName, string folder = "Assets")
            where T : ScriptableObject
    {
        return AssetDatabase.FindAssets($"t:{typeName}", new[] { folder })
            .Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
            .Cast<T>().ToList();
    }

    [MenuItem("Tools/Kill SceneIDMap")]
    public static void KillSceneIDMap()
    {
        Selection.activeGameObject = GameObject.Find("SceneIDMap");
        Debug.Log(Selection.activeGameObject == null ? "SceneID null" : "Found it");
        Object.DestroyImmediate(Selection.activeGameObject);
    }
}
