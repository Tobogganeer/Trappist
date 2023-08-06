using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

public static class LibraryUtil
{
    [MenuItem("AudioManager/Collect Sounds")]
    static void FillSounds()
    {
        FillLibrary<SoundLibrary, Sound>(nameof(SoundLibrary.sounds));
    }

    [MenuItem("Inventory/Collect Items")]
    static void FillItems()
    {
        FillLibrary<ItemLibrary, Item>(nameof(ItemLibrary.items));
    }



    public static void FillLibrary<TLibrary, TObject>(string arrayName)
        where TLibrary : ScriptableObject
        where TObject : ScriptableObject
    {
        TLibrary[] libraries = FindAllScriptableObjectsOfType<TLibrary>();
        if (libraries.Length == 0)
        {
            throw new System.ArgumentException("No libraries found of type " + typeof(TLibrary), "TLibrary");
        }
        if (libraries.Length > 1)
        {
            Debug.LogWarning("More than one library of type " + typeof(TLibrary) + " found, filling only the first (" + libraries[0].name + ")...");
        }

        FillScriptableObjects<TObject>(libraries[0], arrayName);
    }

    public static void FillScriptableObjects<T>(Object owner, string arrayName) where T : ScriptableObject
    {
        SerializedObject ob = new SerializedObject(owner);
        ob.Update();
        SerializedProperty prop = ob.FindProperty(arrayName);
        T[] scrObjects = FindAllScriptableObjectsOfType<T>();
        prop.ClearArray();
        prop.arraySize = scrObjects.Length;
        for (int i = 0; i < scrObjects.Length; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = scrObjects[i];
        }
        ob.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static T[] FindAllScriptableObjectsOfType<T>(string[] folders = null)
            where T : ScriptableObject
    {
        return AssetDatabase.FindAssets($"t:{typeof(T)}", folders)
            .Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
            .Cast<T>().ToArray();
    }
}
