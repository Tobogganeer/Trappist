using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTest : MonoBehaviour
{
    public Terrain t;
    public Material mat;
    public int res = 513;

    [ContextMenu("Set Height")]
    public void Set()
    {
        t.SetHeightmap(mat);//, res);
    }
}
