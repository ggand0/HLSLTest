// ライトマップを使用して、モデルを描画するための色を計算するエフェクト
float4x4 World;
float4x4 View;
float4x4 Projection;

texture2D BasicTexture;
sampler2D basicTextureSampler = sampler_state
{
	texture = <BasicTexture>;
	addressU = wrap;
	addressV = wrap;
	minfilter = anisotropic;
	magfilter = anisotropic;
	mipfilter = linear;
};
bool TextureEnabled = true;
texture2D LightTexture;
sampler2D lightSampler = sampler_state
{
	texture = <LightTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};
float3 AmbientColor = float3(0.15, 0.15, 0.15);
float3 DiffuseColor;
#include "PPShared.vsi"

// projection関係
float4x4 ProjectorViewProjection;
texture2D ProjectedTexture;
sampler2D projectorSampler = sampler_state{
	texture = <ProjectedTexture>;
};
bool ProjectorEnabled = true;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float4 PositionCopy : TEXCOORD1;
	float4 ProjectorScreenPosition : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4x4 worldViewProjection = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, worldViewProjection);
	output.PositionCopy = output.Position;
	output.UV = input.UV;

	// スクリーン座標計算
	output.ProjectorScreenPosition = mul(mul(input.Position, World),
ProjectorViewProjection);

	return output;
}

// 投影されたものが、見える範囲にあるものだけsampleする
float3 sampleProjector(float2 UV)
{
	if (UV.x < 0 || UV.x > 1 || UV.y < 0 || UV.y > 1) {
		return float3(0, 0, 0);
	}
	return tex2D(projectorSampler, UV);
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// ライトマップから輝度値を取得して、元のテクスチャの色と合成するだけ
    // Sample model's texture
	float3 basicTexture = tex2D(basicTextureSampler, input.UV);
	if (!TextureEnabled) {
		basicTexture = float4(1, 1, 1, 1);
	}

	// Extract lighting value from light map
	float2 texCoord = postProjToScreen(input.PositionCopy) + halfPixel();
	float3 light = tex2D(lightSampler, texCoord);
	light += AmbientColor;


	//return float4(basicTexture * DiffuseColor * light, 1);
	float3 projection = float3(0, 0, 0);
	if (ProjectorEnabled) {
		projection = sampleProjector(postProjToScreen(
			input.ProjectorScreenPosition) + halfPixel());
	}
	return float4(basicTexture * DiffuseColor * light + projection, 1);
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
