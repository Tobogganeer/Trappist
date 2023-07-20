using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public static class TerrainUtility
{
    /*
    public static void SetHeightmap(this Terrain t, Texture2D heightmap)
    {
        if (heightmap == null)
            return;

        if (heightmap.width != heightmap.height || !Mathf.IsPowerOfTwo(heightmap.width - 1))
        {
            Debug.LogWarning("Tried to set heightmap to invalid texture! (rectangular or not power of two + 1)");
            return;
        }

        int res = heightmap.width;
        t.terrainData.heightmapResolution = res;

        RenderTexture tempRT = RenderTexture.GetTemporary(res, res);
        TextureUtility.Blit(heightmap, tempRT, GraphicsSettings.currentRenderPipeline.defaultUIMaterial);

        RenderTexture.active = tempRT;

        RectInt source = new RectInt(0, 0, res, res);
        Vector2Int dest = new Vector2Int(0, 0);
        t.terrainData.CopyActiveRenderTextureToHeightmap(source, dest, TerrainHeightmapSyncControl.HeightAndLod);
        //t.terrainData.SyncHeightmap();

        RenderTexture.ReleaseTemporary(tempRT);
        RenderTexture.active = null;
    }

    
    public static void SetHeightmap(this Terrain t, Material material, int resolution)
    {
        if (!Mathf.IsPowerOfTwo(resolution - 1))
        {
            Debug.LogWarning("Terrain resolution must be a power of 2 + 1");
            return;
        }

        Texture2D heightmap = TextureUtility.GenerateTexture(resolution, resolution, material, TextureFormat.RGBA32, true);
        SetHeightmap(t, heightmap);
    }
    */

    public static void SetHeightmap(this Terrain t, Material material)
    {
        int resolution = t.terrainData.heightmapResolution;

        //RenderTexture tempRT = RenderTexture.GetTemporary(resolution, resolution);
        RenderTexture tempRT = RenderTexture.GetTemporary(new RenderTextureDescriptor(resolution, resolution, GraphicsFormat.R16_UNorm, GraphicsFormat.None)
        {
            sRGB = false,
        });

        if (!tempRT.IsCreated())
            tempRT.Create();

        TextureUtility.Blit(Texture2D.blackTexture, tempRT, material);

        RenderTexture.active = tempRT;

        //Texture2D output = new Texture2D(tempRT.width, tempRT.height, GraphicsFormat.R16_UNorm, TextureCreationFlags.None);
        //output.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        //output.Apply();
        //output.filterMode = FilterMode.Bilinear;
        //System.IO.File.WriteAllBytes(Application.dataPath + "/Test.png", output.EncodeToPNG());

        RectInt source = new RectInt(0, 0, resolution, resolution);
        Vector2Int dest = new Vector2Int(0, 0);
        t.terrainData.CopyActiveRenderTextureToHeightmap(source, dest, TerrainHeightmapSyncControl.HeightAndLod);
        //t.terrainData.SyncHeightmap();

        RenderTexture.ReleaseTemporary(tempRT);
        RenderTexture.active = null;
    }

    /*
    public static void SetHeightmap(this Terrain t, Material material)
    {
        int res = t.terrainData.heightmapResolution;

        RenderTexture renderTex = t.terrainData.heightmapTexture;

        Texture2D tex = TextureUtility.GenerateTexture(res, res, material, GraphicsFormat.R16_UNorm, TextureCreationFlags.None);

        Graphics.CopyTexture(tex, renderTex);
        RectInt region = new RectInt(0, 0, res, res);
        t.terrainData.DirtyHeightmapRegion(region, TerrainHeightmapSyncControl.HeightAndLod);
    }
    */
}
