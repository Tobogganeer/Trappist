using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.IconMaker;
using System.Runtime.InteropServices;
using System.IO;

public class RAWExporter : MonoBehaviour
{
    public Material terrainHeightMaterial;
    public Vector2Int size;
    public string path = "Heightmap.raw";

    [ContextMenu("Render")]
    void Render()
    {
        if (terrainHeightMaterial == null)
        {
            Debug.LogWarning("NO MATERIAL!");
            return;
        }

        Texture2D tex = TextureUtility.GenerateTexture(size.x, size.y, terrainHeightMaterial, Texture2D.blackTexture, TextureFormat.RGBAFloat);
        var data = tex.GetRawTextureData<Vector4>();

        //byte[] byteData = new byte[data.Length * 4]; // 4 bytes for every element
        byte[] byteData = new byte[data.Length * 2]; // Try 2

        for (int i = 0; i < data.Length; i++)
        {
            float val = data[i].x;
            val = Mathf.Clamp01(val);
            //int intVal = Mathf.RoundToInt(val * uint.MaxValue);
            int intVal = Mathf.RoundToInt(val * ushort.MaxValue);

            //FloatToBytes bytes = new FloatToBytes();
            //bytes.floatVal = data[i].x;
            IntToBytes bytes = new IntToBytes();
            bytes.intVal = intVal;
            byteData[i * 2] = bytes.byte1;
            byteData[i * 2 + 1] = bytes.byte2;
            //byteData[i * 4] = bytes.byte1;
            //byteData[i * 4 + 1] = bytes.byte2;
            //byteData[i * 4 + 2] = bytes.byte3;
            //byteData[i * 4 + 3] = bytes.byte4;
        }

        string fullPath = Path.Combine(Application.dataPath, path);

        if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        File.WriteAllBytes(fullPath, byteData);
    }

    [StructLayout(LayoutKind.Explicit)]
    struct FloatToBytes
    {
        [FieldOffset(0)]
        public float floatVal;

        [FieldOffset(0)]
        public byte byte1;
        [FieldOffset(1)]
        public byte byte2;
        [FieldOffset(2)]
        public byte byte3;
        [FieldOffset(3)]
        public byte byte4;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct IntToBytes
    {
        [FieldOffset(0)]
        public int intVal;

        [FieldOffset(0)]
        public byte byte1;
        [FieldOffset(1)]
        public byte byte2;
        [FieldOffset(2)]
        public byte byte3;
        [FieldOffset(3)]
        public byte byte4;
    }
}
