using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QualityManager
{
    public static void SetMaxFramerate(int maxFramerate)
    {
        Application.targetFrameRate = maxFramerate;
    }

    public static void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 2 : 0;
    }
}
