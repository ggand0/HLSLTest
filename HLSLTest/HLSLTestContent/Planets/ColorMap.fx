float4x4 wvp : WorldViewProjection;
float4x4 world : World;
float AmbientIntensity = 1;
float4 AmbientColor : AMBIENT = float4(0,0,0,1);

float3 LightDirection : Direction = float3(1,0,1);

texture Palette;
sampler PallSampler = sampler_state 
{
    texture = <Palette>;
    /*magfilter	= POINT;
	minfilter	= POINT;
	mipfilter	= POINT;*/
	magfilter	= LINEAR;
	minfilter	= LINEAR;
	mipfilter	= LINEAR;
	AddressU	= CLAMP;  
	AddressV	= CLAMP;
};
texture ColorMap : Diffuse;
sampler ColorMapSampler = sampler_state 
{
    texture = <ColorMap>;
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= CLAMP;
	AddressV	= CLAMP;
};
texture BumpMap ;
sampler BumpMapSampler = sampler_state
{
	Texture = <BumpMap>;
};
float subtype=0;


int renderType = 0;
texture BaseTexture;// means sea texture
sampler BaseMapSampler = sampler_state
{
	Texture = <BaseTexture>;
	AddressU = Clamp;
	AddressV = Clamp;
};
texture BaseNormalTexture;
sampler BaseNormalSampler = sampler_state
{
	Texture = <BaseNormalTexture>;
};

texture WeightMap;
sampler WeightMapSampler = sampler_state {
	texture = <WeightMap>;
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
};
float TextureTiling = 1;
texture GTexture;
sampler GTextureSampler = sampler_state
{
	texture = <GTexture>;
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};
texture BTexture;
sampler BTextureSampler = sampler_state
{
	texture = <BTexture>;
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};




// Lighting and Shadowing
#include "..\\PPShared.vsi"

texture2D LightTexture;
sampler2D lightSampler = sampler_state
{
	texture = <LightTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};
//float3 AmbientColor = float3(0.15, 0.15, 0.15);
float3 DiffuseColor;

// shadow関係
bool DoShadowMapping = false;
float4x4 ShadowView;
float4x4 ShadowProjection;
texture2D ShadowMap;
sampler2D shadowSampler = sampler_state {
	texture = <ShadowMap>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

// perform the depth comparison
float3 ShadowLightPosition;
float ShadowFarPlane;
float ShadowMult = 0.3f;
float ShadowBias =  0.1f;


struct VertexShaderInput
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 Light : TEXCOORD1;

	float4 PositionCopy : TEXCOORD2;
	float4 ShadowScreenPosition : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Position = mul(input.Position, wvp);	
	//output.Light = LightDirection;
	output.TexCoord = input.TexCoord;
	// output.Normal = mul(input.Normal, world);

	 
	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(input.Tangent, world);
	worldToTangentSpace[1] = mul(cross(input.Tangent, input.Normal), world);
	worldToTangentSpace[2] = mul(input.Normal, world);
	
	output.Light = mul(worldToTangentSpace, LightDirection);	
	output.PositionCopy = output.Position;
	output.ShadowScreenPosition = mul(mul(input.Position, world),
		mul(ShadowView, ShadowProjection));
	
    return output;
}


float2 sampleShadowMap(float2 UV)
{
	if (UV.x < 0 || UV.x > 1 || UV.y < 0 || UV.y > 1) {
		return float2(1, 1);
	}

	float2 debug = tex2D(shadowSampler, UV).rg;
	return debug;
	//return tex2D(shadowSampler, UV).rg;// returns read and green value
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 output = float4(0,0,0,0);
	
	float3 Normal = (2 * (tex2D(BumpMapSampler, input.TexCoord))) - 1.0;
	float3 LightDir = normalize(input.Light);
	float Diffuse = saturate(dot(LightDir, Normal));


	if (renderType == 0) {
		float4 height = tex2D(ColorMapSampler, input.TexCoord);
		float4 texCol = tex2D(PallSampler, float2(subtype, height.x));
		texCol *= Diffuse;
		//output =  AmbientColor + texCol;
		output = texCol;
	} else if (renderType == 1)  {
		// trying multi-texturing
		// 赤をbaseTexにしていることに注意
		float4 BaseColor = tex2D(BaseMapSampler, input.TexCoord);
		float4 weightMap = tex2D(WeightMapSampler, input.TexCoord);
		float4 gTex = tex2D(GTextureSampler, input.TexCoord * TextureTiling);// TT分引き伸ばされる？
		float4 bTex = tex2D(BTextureSampler, input.TexCoord * TextureTiling);

		output = clamp(1.0f - weightMap.r - weightMap.g - weightMap.b, 0, 1);
		output += weightMap.r * BaseColor + weightMap.g * gTex+ weightMap.b * bTex;

		float4 height = tex2D(ColorMapSampler, input.TexCoord);
		float4 texCol = tex2D(PallSampler, float2(subtype, height.x));
		//texCol *= Diffuse;
		//output += AmbientColor;
	} else if (renderType == 2) {
		// Setup sea texture + multi coloring
		float4 BaseColor = tex2D(BaseMapSampler, input.TexCoord);
		float4 weightMap = tex2D(WeightMapSampler, input.TexCoord);

		float3 NormalTex = (2 * (tex2D(BaseNormalSampler, input.TexCoord))) - 1.0;
		float DiffuseTex = saturate(dot(LightDir, Normal));// 怪しい
		//BaseColor *= DiffuseTex;

		float4 height = tex2D(ColorMapSampler, input.TexCoord);
		float4 texCol = tex2D(PallSampler, float2(subtype, height.x));
		//texCol *= Diffuse;

		output += weightMap.r * BaseColor * texCol + weightMap.g * texCol;
		output += AmbientColor;
	}



	// ProjectShadowDepthEffectV4のピクセルシェーダをコピー。
	// 余裕が出来たら両者をfxhにまとめる。

	// Extract lighting value from light map
	float2 texCoord = postProjToScreen(input.PositionCopy) + halfPixel();
	float3 light = tex2D(lightSampler, texCoord);
	light += AmbientColor;


	float shadow = 1;
	if (DoShadowMapping) {
		float2 shadowTexCoord = postProjToScreen(input.ShadowScreenPosition)
			+ halfPixel();
		float ShadowBias = 1.0f;//0.001f
		float realDepth = input.ShadowScreenPosition.z / ShadowFarPlane - ShadowBias;

		if (realDepth < 1) {// realDepth > momentsなら描画？
			// Variance shadow mapping code below from the variance shadow
			// mapping demo code @ http://www.punkuser.net/vsm/

			// Sample from depth texture
			float2 moments = sampleShadowMap(shadowTexCoord);

			// check if we're in shadow
			// momentsは元々depthを出力させているのでどちらの要素も同じ
			float lit_factor = (realDepth <= moments.x);// ピクセルが影かどうか：つまり0 or 1

			// Variance shadow mapping
			float E_x2 = moments.y;									// = E(x^2)
			float Ex_2 = moments.x * moments.x;						// (E(x))^2

			// 分散 = s^2 = E(x^2) - (E(x))^2 = M_2 - (M_1)^2 (pdfより)
			//float variance = min(max(E_x2 - Ex_2, 0.0) + 1.0f / 10000.0f, 1.0);
			float variance = min(max(E_x2 - Ex_2, 0.0) + 1.0f / 10000.0f, 1.0);
			float m_d = (moments.x - realDepth);					// =t-μ
			float p = variance / (variance + m_d * m_d);			// pdfの(5)式 p=1になってしまう←floatをfloat2にいれていたせい

			shadow = clamp(max(lit_factor, p), ShadowMult, 1.0f);
		}
	}


	//float4 finalColor = float4(basicTexture * DiffuseColor * light * shadow, 1);
	float4 finalColor = float4(output * light * shadow, 1);
	//float4 finalColor = float4(output * shadow);// とりあえず
	return finalColor;


    return output;
}

technique Technique1
{
    pass Pass1
    {
		// shader code uses too many arithmetic instruction slotsより3.0に変更。どのみちほかのシェーダーで必要になるので。
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}