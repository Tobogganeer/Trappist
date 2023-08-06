using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public static class TextureUtility
{
    public static Texture2D GenerateTexture(int width, int height, Material mat, TextureFormat format = TextureFormat.RGBA32, bool linear = false)
    {
        return GenerateTexture(width, height, mat, Texture2D.blackTexture, format, linear);
    }

    public static Texture2D GenerateTextureFormat(int width, int height, Material mat, GraphicsFormat format, TextureCreationFlags flags = TextureCreationFlags.None)
    {
        // Should probably make more generic but idc

        RenderTexture tempRT = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(Texture2D.blackTexture, tempRT, mat, 0, 0); // HDRP
        //Blit(Texture2D.blackTexture, tempRT, mat);

        Texture2D output = new Texture2D(tempRT.width, tempRT.height, format, flags);
        RenderTexture.active = tempRT;

        output.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        output.Apply();
        output.filterMode = FilterMode.Bilinear;

        RenderTexture.ReleaseTemporary(tempRT);
        RenderTexture.active = null;

        return output;
    }

    public static Texture2D GenerateTexture(int width, int height, Material mat, Texture2D tex, TextureFormat format = TextureFormat.RGBA32, bool linear = false)
    {
        RenderTexture tempRT = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(tex, tempRT, mat, 0, 0);
        //Blit(tex, tempRT, mat);

        Texture2D output = new Texture2D(tempRT.width, tempRT.height, format, false, linear);
        RenderTexture.active = tempRT;

        output.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        output.filterMode = FilterMode.Bilinear;
        output.Apply();

        RenderTexture.ReleaseTemporary(tempRT);
        RenderTexture.active = null;

        return output;
    }

    // https://forum.unity.com/threads/forcing-graphics-blit-to-work-can-you-help-me.962373/
    public static void Blit(Texture source, RenderTexture dest, Material mat, int pass = 0, bool executeImmediately = true)
    {
        var original = RenderTexture.active;
        RenderTexture.active = dest;   /* or Graphics.SetRenderTarget(..) */

        if (mat != null)
            mat.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        // activate the first shader pass (in this case we know it is the only pass)
        if (mat != null)
            mat.SetPass(pass);
        // draw a quad over whole screen
        GL.Begin(GL.QUADS);
        GL.TexCoord2(0f, 0f); GL.Vertex3(0f, 0f, 0f);    /* note the order! */
        GL.TexCoord2(0f, 1f); GL.Vertex3(0f, 1f, 0f);    /* also, we need TexCoord2 */
        GL.TexCoord2(1f, 1f); GL.Vertex3(1f, 1f, 0f);
        GL.TexCoord2(1f, 0f); GL.Vertex3(1f, 0f, 0f);
        GL.End();
        GL.PopMatrix();
        if (executeImmediately)
            GL.Flush();
        RenderTexture.active = original;    /* restore */
    }

    public static Texture2D GenerateColourTexture(Color colour, int resolution = 4, bool apply = true, TextureFormat format = TextureFormat.RGB24)
    {
        Texture2D texture = new Texture2D(resolution, resolution, format, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] colours = new Color[resolution * resolution];
        for (int i = 0; i < colours.Length; i++)
            colours[i] = colour;
        texture.SetPixels(colours);
        if (apply)
            texture.Apply();
        return texture;
    }
}
