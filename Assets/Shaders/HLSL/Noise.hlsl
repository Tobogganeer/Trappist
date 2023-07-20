#ifndef INCLUDE_TOBO_TERRAIN_NOISE
#define INCLUDE_TOBO_TERRAIN_NOISE

#include "FastNoiseLite.hlsl"

void WarpNoise(fnl_state noise, inout float x, inout float y)
{
    float qX = x;
    float qY = y;

    fnlDomainWarp2D(noise, qX, qY);

    x = qX;
    y = qY;

    //float rX = x + 4 * qX + 1.7f;
    //float rY = y + 4 * qY + 9.2f;

    //fnlDomainWarp2D(noise, rX, rY);

    //x += 4 * rX;
    //y += 4 * rY;
}

void Noise_float(float2 uv, int type, int seed, float freq, int octaves, float laq, float gain, float warpAmp, out float value)
{
    fnl_state state = fnlCreateState(seed);
    //prec.frequency = (_PFreq * (_Freq / 10)) / 10000;
    state.frequency = freq;
    state.noise_type = FNL_NOISE_OPENSIMPLEX2;
    state.rotation_type_3d = FNL_ROTATION_NONE;

    //state.fractal_type = FNL_FRACTAL_FBM; // FNL_FRACTAL_NONE
    state.fractal_type = type;
    state.octaves = octaves;
    state.lacunarity = laq;
    state.gain = gain;

    //state.cellular_distance_func = FNL_CELLULAR_DISTANCE_HYBRID;
    //state.cellular_return_type = FNL_CELLULAR_RETURN_TYPE_DISTANCE;

    state.domain_warp_amp = warpAmp;
    
    float x = uv.x;
    float y = uv.y;

    WarpNoise(state, x, y);

    value = (fnlGetNoise2D(state, x, y) + 1.0f) / 2.0f;
}

#endif