//UNITY_SHADER_NO_UPGRADE
#ifndef INCLUDE_TOBO_GRASS_BUFFER
#define INCLUDE_TOBO_GRASS_BUFFER

//#include "UnityCG.cginc"
//#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
//#include "UnityIndirect.cginc"

struct MeshProperties
{
    //float4 posWS;
    float4x4 mat;
    //float4x4 matInverse;
    //float4 color;
};


StructuredBuffer<MeshProperties> _Properties;

// https://www.youtube.com/watch?v=bny9f4zw5JE

// https://gist.github.com/Cyanilux/4046e7bf3725b8f64761bf6cf54a16eb

//#indef SHADERGRAPH_PREVIEW


void vertInstancingMatrices(inout float4x4 objectToWorld, out float4x4 worldToObject)
{
#if UNITY_ANY_INSTANCING_ENABLED
	MeshProperties data = _Properties[unity_InstanceID];

	//objectToWorld = mul(objectToWorld, data.mat);
    objectToWorld = data.mat;
    //worldToObject = unity_WorldToObject;

    //return;

	// Transform matrix (override current)
	// I prefer keeping positions relative to the bounds passed into DrawMeshInstancedIndirect so use the above instead
	//objectToWorld._11_21_31_41 = float4(data.m._11_21_31, 0.0f);
	//objectToWorld._12_22_32_42 = float4(data.m._12_22_32, 0.0f);
	//objectToWorld._13_23_33_43 = float4(data.m._13_23_33, 0.0f);
	//objectToWorld._14_24_34_44 = float4(data.m._14_24_34, 1.0f);

	// Inverse transform matrix
	float3x3 w2oRotation;
	w2oRotation[0] = objectToWorld[1].yzx * objectToWorld[2].zxy - objectToWorld[1].zxy * objectToWorld[2].yzx;
	w2oRotation[1] = objectToWorld[0].zxy * objectToWorld[2].yzx - objectToWorld[0].yzx * objectToWorld[2].zxy;
	w2oRotation[2] = objectToWorld[0].yzx * objectToWorld[1].zxy - objectToWorld[0].zxy * objectToWorld[1].yzx;

	float det = dot(objectToWorld[0].xyz, w2oRotation[0]);
	w2oRotation = transpose(w2oRotation);
	w2oRotation *= rcp(det);
	float3 w2oPosition = mul(w2oRotation, -objectToWorld._14_24_34);

	worldToObject._11_21_31_41 = float4(w2oRotation._11_21_31, 0.0f);
	worldToObject._12_22_32_42 = float4(w2oRotation._12_22_32, 0.0f);
	worldToObject._13_23_33_43 = float4(w2oRotation._13_23_33, 0.0f);
	worldToObject._14_24_34_44 = float4(w2oPosition, 1.0f);
#endif
}


void instancingSetup()
{
    #ifndef SHADERGRAPH_PREVIEW
    #if UNITY_ANY_INSTANCING_ENABLED
    //vertInstancingMatrices(unity_ObjectToWorld, unity_WorldToObject);
    float4x4 m = UNITY_MATRIX_M;
    float4x4 im = UNITY_MATRIX_I_M;
    vertInstancingMatrices(m, im);
    UNITY_MATRIX_M = m;
    UNITY_MATRIX_I_M = im;
    #endif
    #endif
}

void GetInstanceID_float(out float Out)
{
    Out = 0;
    #ifndef SHADERGRAPH_PREVIEW
    #if UNITY_ANY_INSTANCING_ENABLED
    Out = unity_InstanceID;
    #endif
    #endif
}

void Instancing_float(float3 position, out float3 Out)
{
    Out = position;
}

void PositionWS_float(float instanceID, out float3 Out)
{
    Out = 0;
    #ifndef SHADERGRAPH_PREVIEW
    #if UNITY_ANY_INSTANCING_ENABLED
    //Out = _Properties[instanceID].posWS.xyz;
    #endif
    #endif
}




/*
// https://forum.unity.com/threads/shadergraph-instancing.1339943/
// use #pragma instancing_options procedural:vertInstancingSetup to setup unity_InstanceID & related macro, what vertInstancingSetup do are actually not important
#if UNITY_ANY_INSTANCING_ENABLED
    void instancingSetup() { }
#endif

void GetPosition_float(in float3 positionOS, out float3 Out)
{
    Out = 0;
    #if UNITY_ANY_INSTANCING_ENABLED
    //Out = positionOS + _Properties[unity_InstanceID].Position.xyz;
    float4 wpos = mul(_Properties[unity_InstanceID].mat, positionOS);
    Out = wpos;
    //Out = mul(UNITY_MATRIX_VP, wpos);
    #endif
	//InitIndirectDrawArgs(0);
	//uint cmdID = GetCommandID(0);
    //uint instanceID = GetIndirectInstanceID(svInstanceID);
    //float4 wpos = mul(_Properties[instanceID].mat, positionWS);
    //Out = mul(UNITY_MATRIX_VP, wpos);
}
*/

#endif