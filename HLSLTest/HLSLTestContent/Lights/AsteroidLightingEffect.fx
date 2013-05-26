float4x4 World;
float4x4 View;
float4x4 Projection;

texture BasicTexture;
// テクスチャの色をtex2D関数を使って取得するために、
// テクスチャからどのように色を取り出す（サンプルする）かを表すサンプラを宣言しなければならない：
sampler BasicTextureSampler = sampler_state {
	texture = <BasicTexture>;

	MinFilter = Anisotropic;// 異方性フィルタ
	MagFilter = Anisotropic;
	MipFilter = Linear;		// Mip-mapping
	// U軸及びV軸方向のテクスチャアドレッシングモード（address mode）。サイズを変えた時（座標に(0, 1)を超える値を与えた時）の振る舞いで、
	// clampなら引き伸ばされ、wrapなら位置が変わるだけ？
	AddressU = Wrap;
	AddressV = Wrap;
};
bool TextureEnabled = false;
float3 DiffuseColor = float3(1, 1, 1);
float3 AmbientLightColor = float3(.2, .2, .2);
float3 AmbientColor = float3(.2, .2, .2);
float3 LightDirection = float3(1, 1, 1);
float3 LightColor = float3(0.9, 0.9, 0.9);
float SpecularPower = 32;
float3 SpecularColor = float3(1, 1, 1);
float3 CameraPosition;

float4 AccentColor;


#include "..//PPShared.vsi"
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
float ShadowBias =  0.001f;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;// テクスチャ座標
	float3 Normal : NORMAL0;// 法線
};

struct VertexShaderOutput
{
	// TODO: ここにカラーおよびテクスチャー座標などの頂点シェーダーの
    // 出力要素を追加します。これらの値は該当する三角形上で自動的に補間されて、
    // ピクセル シェーダーへの入力として提供されます。
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
	float4 ShadowScreenPosition : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.Normal = mul(input.Normal, World);

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // TODO: ここで頂点シェーダー コードを追加します。
	output.UV = input.UV;// テクスチャを貼るだけなので場所は変えない
	output.ViewDirection = worldPosition - CameraPosition;


	output.ShadowScreenPosition = mul(mul(input.Position, World),
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
	// Start with diffuse color
	float3 color = float3(1,1,1);//DiffuseColor;

	// Texture if necessary
	if (TextureEnabled) {
		// 対応するピクセルのテクスチャの色を持ってくるためにtex2D関数を使用
		color *= tex2D(BasicTextureSampler, input.UV);
	}
	// add lighter color for asteroid
	//color += float3(0.1f, 0.1f, 0.1f);
	//color += float3(0.3f, 0.3f, 0.3f);
	//color += AccentColor * 0.2f;

	// Start with ambient lighting
	float3 lighting = AmbientColor;
	float3 lightDir = normalize(LightDirection);
	float3 normal = normalize(input.Normal);

	// Add lambertian lighting
	lighting += saturate(dot(lightDir, normal)) * LightColor;

	float3 refl = reflect(lightDir, normal);
	float3 view = normalize(input.ViewDirection);
	// Add specular highlights
	lighting += pow(saturate(dot(refl, view)), SpecularPower) * SpecularColor;

	// Scaleが小さい場合宇宙空間の背景では見えないため、単にモデル全体を明るくすることにした
	//float3 lighting = float3(1,1,1);


	float shadow = 1;
	if (DoShadowMapping) {
		float2 shadowTexCoord = postProjToScreen(input.ShadowScreenPosition)
			+ halfPixel();
		float ShadowBias = 0.001f;//0.001f
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


	// Calculate final color
	float3 output = saturate(lighting) * color * shadow;
	float4 d = float4(0,0,0,0);
	d += AccentColor;
	//return float4(1,0,0,1);
	return AccentColor.rgba;
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
