//UNITY_SHADER_NO_UPGRADE
#ifndef INCLUDE_TOBO_NOISES
#define INCLUDE_TOBO_NOISES

// Cellular noise ("Worley noise") in 2D in GLSL.
// Copyright (c) Stefan Gustavson 2011-04-19. All rights reserved.
// This code is released under the conditions of the MIT license.
// See LICENSE file for details.
// https://github.com/stegu/webgl-noise

// Modulo 289 without a division (only multiplications)
float3 mod289(float3 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float2 mod289(float2 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

// Modulo 7 without a division
float3 mod7(float3 x) {
  return x - floor(x * (1.0 / 7.0)) * 7.0;
}

// Permutation polynomial: (34x^2 + x) mod 289
float3 permute(float3 x) {
  return mod289((34.0 * x + 1.0) * x);
}

// Cellular noise, returning F1 and F2 in a float2.
// Standard 3x3 search window for good F1 and F2 values
float2 cellular(float2 P) {
#define K 0.142857142857 // 1/7
#define Ko 0.428571428571 // 3/7
#define jitter 1.0 // Less gives more regular pattern
	float2 Pi = mod289(floor(P));
 	float2 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = permute(Pi.x + oi);
	float3 p = permute(px.x + Pi.y + oi); // p11, p12, p13
	float3 ox = frac(p*K) - Ko;
	float3 oy = mod7(floor(p*K))*K - Ko;
	float3 dx = Pf.x + 0.5 + jitter*ox;
	float3 dy = Pf.y - of + jitter*oy;
	float3 d1 = dx * dx + dy * dy; // d11, d12 and d13, squared
	p = permute(px.y + Pi.y + oi); // p21, p22, p23
	ox = frac(p*K) - Ko;
	oy = mod7(floor(p*K))*K - Ko;
	dx = Pf.x - 0.5 + jitter*ox;
	dy = Pf.y - of + jitter*oy;
	float3 d2 = dx * dx + dy * dy; // d21, d22 and d23, squared
	p = permute(px.z + Pi.y + oi); // p31, p32, p33
	ox = frac(p*K) - Ko;
	oy = mod7(floor(p*K))*K - Ko;
	dx = Pf.x - 1.5 + jitter*ox;
	dy = Pf.y - of + jitter*oy;
	float3 d3 = dx * dx + dy * dy; // d31, d32 and d33, squared
	// Sort out the two smallest distances (F1, F2)
	float3 d1a = min(d1, d2);
	d2 = max(d1, d2); // Swap to keep candidates for F2
	d2 = min(d2, d3); // neither F1 nor F2 are now in d3
	d1 = min(d1a, d2); // F1 is now in d1
	d2 = max(d1a, d2); // Swap to keep candidates for F2
	d1.xy = (d1.x < d1.y) ? d1.xy : d1.yx; // Swap if smaller
	d1.xz = (d1.x < d1.z) ? d1.xz : d1.zx; // F1 is in d1.x
	d1.yz = min(d1.yz, d2.yz); // F2 is now not in d2.yz
	d1.y = min(d1.y, d1.z); // nor in  d1.z
	d1.y = min(d1.y, d2.x); // F2 is in d1.y, we're done.
	return sqrt(d1.xy);
}

void Worley_float(float2 uv, out float2 value)
{
	value = cellular(uv);
}




// https://www.shadertoy.com/view/Xd23Dh
float3 hash3( float2 p )
{
    float3 q = float3( dot(p,float2(127.1,311.7)), 
				   dot(p,float2(269.5,183.3)), 
				   dot(p,float2(419.2,371.9)) );
	return frac(sin(q)*43758.5453);
}

float voronoise( in float2 p, float u, float v )
{
	float k = 1.0+63.0*pow(1.0-v,6.0);

    float2 i = floor(p);
    float2 f = frac(p);
    
	float2 a = float2(0.0,0.0);
	[unroll]
    for( int y=-2; y<=2; y++ )
	[unroll]
    for( int x=-2; x<=2; x++ )
    {
        float2  g = float2( x, y );
		float3  o = hash3( i + g )*float3(u,u,1.0);
		float2  d = g - f + o.xy;
		float w = pow( 1.0-smoothstep(0.0,1.414,length(d)), k );
		a += float2(o.z*w,w);
    }
	
    return a.x/a.y;
}

void Voronoise_float(float2 uv, float2 p, out float value)
{
	value = voronoise(uv, p.x, p.y);
}

// https://www.shadertoy.com/view/tdG3Rd

float rand(float2 n) { 
    return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
}

float noise(float2 p){
    float2 ip = floor(p);
    float2 u = frac(p);
    u = u*u*(3.0-2.0*u);

    float res = lerp(
        lerp(rand(ip),rand(ip+float2(1.0,0.0)),u.x),
        lerp(rand(ip+float2(0.0,1.0)),rand(ip+float2(1.0,1.0)),u.x),u.y);
    return res*res;
}

const float2x2 mtx = float2x2( 0.80,  0.60, -0.60,  0.80 );

float fbm( float2 p )
{
    float f = 0.0;

    f += 0.500000*noise( p ); p = mul(mtx, p)*2.02;
    f += 0.031250*noise( p ); p = mul(mtx, p)*2.01;
    f += 0.250000*noise( p ); p = mul(mtx, p)*2.03;
    f += 0.125000*noise( p ); p = mul(mtx, p)*2.01;
    f += 0.062500*noise( p ); p = mul(mtx, p)*2.04;
    f += 0.015625*noise( p );

    return f/0.96875;
}

float pattern( in float2 p, in float2 warp, out float2 r, out float2 q)
{
	q = float2( fbm( p + float2(warp.x * warp.y,0.0) ),
                   fbm( p + float2(5.2,1.3) * warp ) );

    r = float2( fbm( p + 4.0*q + float2(1.7,9.2) * warp.x ),
                   fbm( p + 4.0*q + float2(8.3,2.8) * warp.y ) );

    return fbm( p + 4.0*r );
	
	//return fbm( p + fbm( p + fbm( p, mod ), mod ), mod );
}

void FBMWarp_float(float2 uv, float2 warp, out float value, out float2 r, out float2 q)
{
	value = pattern(uv, warp, r, q);
}

#endif