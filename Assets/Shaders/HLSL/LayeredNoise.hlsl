//UNITY_SHADER_NO_UPGRADE
#ifndef INCLUDE_LAYERED_NOISE
#define INCLUDE_LAYERED_NOISE

// Stolen from Unity (thanks bro)
float2 STOLEN_Unity_GradientNoise_Dir_float(float2 p)
{
            // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
    p = p % 289;
            // need full precision, otherwise half overflows when p > 1
    float x = float(34 * p.x + 1) * p.x % 289 + p.y;
    x = (34 * x + 1) * x % 289;
    x = frac(x / 41) * 2 - 1;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}
        
void STOLEN_Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
{
    float2 p = UV * Scale;
    float2 ip = floor(p);
    float2 fp = frac(p);
    float d00 = dot(STOLEN_Unity_GradientNoise_Dir_float(ip), fp);
    float d01 = dot(STOLEN_Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
    float d10 = dot(STOLEN_Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
    float d11 = dot(STOLEN_Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
    Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
}

void LayeredNoise_float(float2 uv, float scale, float persistence, float roughness, out float value)
{
    #define OCTAVES 6

    float noise = 0;
    float freq = scale;
    float amp = 1;
    float totalAmp = 0;
    //float2 randOff = float2(0, 0);
    
    [unroll]
    for (int i = 0; i < OCTAVES; i++)
    {
        float v;
        STOLEN_Unity_GradientNoise_float(uv, freq, v);
        noise += (v + 1) * 0.5f * amp;
        freq *= roughness;
        totalAmp += amp;
        amp *= persistence;
        //randOff += float2(1923.1288, 1729.9126) * freq;
    }

    value = noise / totalAmp;
    
    /*
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < settings.numLayers; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.centre);
            noiseValue += (v + 1) * .5f * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    */
}

#endif