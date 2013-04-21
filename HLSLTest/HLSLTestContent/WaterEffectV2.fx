float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;
float4x4 ReflectedView;

texture ReflectionMap;
sampler2D reflectionSampler = sampler_state {
	texture = <ReflectionMap>;
	//MinFilter = Anisotropic;
	//MagFilter = Anisotropic;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Mirror;
	AddressV = Mirror;
};
#include "PPShared.vsi"

texture WaterNormalMap;
	sampler2D waterNormalSampler = sampler_state {
	texture = <WaterNormalMap>;
};
float WaveLength = 0.6;
float WaveHeight = 0.2;
float Time = 0;
float WaveSpeed = 0.04f;

float3 LightDirection = float3(1, 1, 1);

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 ReflectionPosition : TEXCOORD0;
	float2 NormalMapPosition : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	// from the main camera's point of view
	float4x4 wvp = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, wvp);

	// from the reflected camera's point of view
	float4x4 rwvp = mul(World, mul(ReflectedView, Projection));
	output.ReflectionPosition = mul(input.Position, rwvp);
	//output.UV = input.UV;

	output.NormalMapPosition = input.UV/WaveLength;
	output.NormalMapPosition.y -= Time * WaveSpeed;
	output.WorldPosition = mul(input.Position, World);

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 reflectionUV = postProjToScreen(input.ReflectionPosition) +
		halfPixel();
	//float3 reflection = tex2D(reflectionSampler, reflectionUV);
	float3 BaseColor = float3(0.2, 0.2, 0.8);
	float BaseColorAmount = 0.3f;

	float4 normal = tex2D(waterNormalSampler, input.NormalMapPosition) * 2 - 1;
	float2 UVOffset = WaveHeight * normal.rg;
	float3 reflection = tex2D(reflectionSampler, reflectionUV + UVOffset);

	//return float4(reflection, 1);
	//return float4(lerp(reflection, BaseColor, BaseColorAmount), 1);

	float3 viewDirection = normalize(CameraPosition - input.WorldPosition);
	float3 reflectionVector = -reflect(LightDirection, normal.rgb);
	float specular = dot(normalize(reflectionVector), viewDirection);
	specular = pow(specular, 256);

	return float4(lerp(reflection, BaseColor, BaseColorAmount) + specular, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: ここでレンダーステートを設定します。

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
