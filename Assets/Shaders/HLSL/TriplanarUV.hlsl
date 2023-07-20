//UNITY_SHADER_NO_UPGRADE
#ifndef INCLUDE_TRIPLANAR_UV
#define INCLUDE_TRIPLANAR_UV

// https://catlikecoding.com/unity/tutorials/advanced-rendering/triplanar-mapping/

float3 GetTriplanarWeights(float3 normal)
{
    float3 triW = abs(normal);
    return triW / (triW.x + triW.y + triW.z);
}

struct TriplanarUV
{
    float2 x, y, z;
};

TriplanarUV GetTriplanarUV(float3 position, float3 normal)
{
    TriplanarUV triUV;
    float3 p = position;
    triUV.x = p.zy;
    triUV.y = p.xz;
    triUV.z = p.xy;
    if (normal.x < 0)
    {
        triUV.x.x = -triUV.x.x;
    }
    if (normal.y < 0)
    {
        triUV.y.x = -triUV.y.x;
    }
    if (normal.z >= 0)
    {
        triUV.z.x = -triUV.z.x;
    }
    return triUV;
}




void TriplanarUV_float(float3 position, float3 normal,
    out float2 uvX,
    out float2 uvY,
    out float2 uvZ,
    out float3 weights)
{
    weights = GetTriplanarWeights(normal);
    TriplanarUV triUV = GetTriplanarUV(position, normal);
    
    uvX = triUV.x;
    uvY = triUV.y;
    uvZ = triUV.z;
}

#endif