using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Terrain terrain;
    public Material heightMaterial;
    public Material colourGenMaterial;
    public int colourTexRes = 4096;
    public Material shadingMaterial;

    [ContextMenu("Generate")]
    public void Generate()
    {
        terrain.SetHeightmap(heightMaterial);//, res);
        Texture2D heightmap = TextureUtility.GenerateTexture(colourTexRes, colourTexRes, heightMaterial, TextureFormat.RGB24, true);
        shadingMaterial.mainTexture = TextureUtility.GenerateTexture(colourTexRes, colourTexRes, colourGenMaterial, heightmap);
        //shadingMaterial.mainTexture = heightmap;
    }
}
