using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ease
{
    // https://www.youtube.com/watch?v=mr5xkf6zSzk

    public static float SmoothStart2(float value)
    {
        return value * value;
    }
    public static float SmoothStart3(float value)
    {
        return value * value * value;
    }
    public static float SmoothStart4(float value)
    {
        return value * value * value * value;
    }
    public static float SmoothStart5(float value)
    {
        return value* value *value * value * value;
    }

    public static float SmoothStop2(float value)
    {
        float flip = 1 - value;
        return 1 - (flip * flip);
    }
    public static float SmoothStop3(float value)
    {
        float flip = 1 - value;
        return 1 - (flip * flip * flip);
    }
    public static float SmoothStop4(float value)
    {
        float flip = 1 - value;
        return 1 - (flip * flip * flip * flip);
    }
    public static float SmoothStop5(float value)
    {
        float flip = 1 - value;
        return 1 - (flip * flip * flip * flip * flip);
    }

    public static float SmoothStep2(float value)
    {
        return Mathf.Lerp(SmoothStart2(value), SmoothStop2(value), value);
    }
    public static float SmoothStep3(float value)
    {
        return Mathf.Lerp(SmoothStart3(value), SmoothStop3(value), value);
    }
    public static float SmoothStep4(float value)
    {
        return Mathf.Lerp(SmoothStart4(value), SmoothStop4(value), value);
    }
    public static float SmoothStep5(float value)
    {
        return Mathf.Lerp(SmoothStart5(value), SmoothStop5(value), value);
    }
}
