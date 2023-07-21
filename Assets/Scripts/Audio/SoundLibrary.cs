using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[CreateAssetMenu(menuName = "Scriptable Objects/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    // Doing this because I was getting some weird serialization bugs when
    //  AudioManager just had a Sound[]
    // Probably because it was in a prefab in resources or smth, idk

    [Header("Fill through Menu")]
    public Sound[] sounds;


#if UNITY_EDITOR

    [MenuItem("AudioManager/Collect Sounds")]
    static void FillFromToolbar()
    {
        FindAllScriptableObjectsOfType<SoundLibrary>()[0].FillSounds();
    }

    [ContextMenu("Fill Sounds")]
    void FillSounds()
    {
        SerializedObject ob = new SerializedObject(this);
        ob.Update();
        SerializedProperty prop = ob.FindProperty(nameof(sounds));
        //sounds = FindAllScriptableObjectsOfType<Sound>();
        //if (sounds.Length == 0)
        //    Debug.LogWarning("Didn't find any sounds.");
        Sound[] s = FindAllScriptableObjectsOfType<Sound>();
        prop.ClearArray();
        prop.arraySize = s.Length;
        for (int i = 0; i < s.Length; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = s[i];
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
#endif
}
