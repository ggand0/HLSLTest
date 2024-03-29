// 法線マップと深度マップから、ライトマップ（light map）を作成するエフェクト（シェーダー）
float4x4 WorldViewProjection;
float4x4 InvViewProjection;

texture2D DepthTexture;	// 深度（距離）マップ
texture2D NormalTexture;// 法線マップ
sampler2D depthSampler = sampler_state
{
	texture = <DepthTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};
sampler2D normalSampler = sampler_state
{
	texture = <NormalTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

float3 LightColor;
float3 LightPosition;
float LightAttenuation;
// Include shared functions
#include "PPShared.vsi"
float3 FrustumCorners[4];
struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 LightPosition : TEXCOORD0;
	float3 FrustumRay : TEXCOORD1;
};
float3 GetFrustumRay(in float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = mul(input.Position, WorldViewProjection);
	output.LightPosition = output.Position;
	output.FrustumRay = GetFrustumRay(input.TexCoord);

    return output;
}



float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // 深度マップを使って、オリジナルのworld positionを再構成する
	// それから、その位置と法線の値を用いて輝度の計算を行う。

	// Find the pixel coordinates of the input position in the depth
	// and normal textures
	float2 texCoord = postProjToScreen(input.LightPosition) + halfPixel();

	// Extract the depth for this pixel from the depth map
	float4 depth = tex2D(depthSampler, texCoord);

	// Recreate the position with the UV coordinates and depth value
	float4 position;
	position.x = texCoord.x * 2 - 1;
	position.y = (1 - texCoord.y) * 2 - 1;
	position.z = depth.r;
	position.w = 1.0f;

	/*float3 pos = input.FrustumRay * depth.r;
	position = float4(pos, 1);
	position.w = 1.0f;*/

	// Transform position from screen space to world space
	position = mul(position, InvViewProjection);
	position.xyz /= position.w;

	// Extract the normal from the normal map and move from
	// 0 to 1 range to -1 to 1 range
	float4 normal = (tex2D(normalSampler, texCoord) - .5) * 2;


	// ここまで来たら後は通常の輝度計算をするのみ
	// Perform the lighting calculations for a point light
	float3 lightDirection = normalize(LightPosition - position);
	float lighting = clamp(dot(normal, lightDirection), 0, 1);// 角度で大体決まってるので、範囲を広くしたい場合は光源を遠くに置く必要有


	// Attenuate the light to simulate a point light
	float d = distance(LightPosition, position);
	float att = 1 - pow(d / LightAttenuation, 6);
	//float att = saturate(1.0f - d / 10000);// もっと範囲が広いライトが欲しいので変更。後々パラメータ化
	//float att = saturate(1.0f - log(d) / 100000.0f / 10000);// 線形よりもっと緩く
	//float att = 1.0f;

	return float4(LightColor * lighting * att, 1);
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
