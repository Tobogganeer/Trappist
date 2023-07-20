using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MiscExtensions
{
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
