float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;
float4x4 ReflectedView;

texture ReflectionMap;
sampler2D reflectionSampler = sampler_state {
	texture = <ReflectionMap>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};
#include "PPShared.vsi"

struct VertexShaderInput
{
    float4 Position : POSITION0;
	//float2 UV : TEXCOORD0;// テクスチャ座標
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 ReflectionPosition : TEXCOORD0;
	//float2 UV : TEXCOORD0;// テクスチャ座標
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

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 reflectionUV = postProjToScreen(input.ReflectionPosition) +
		halfPixel();
	float3 reflection = tex2D(reflectionSampler, reflectionUV);
	//float3 reflection = tex2D(reflectionSampler, input.UV);

	return float4(reflection, 1);
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
