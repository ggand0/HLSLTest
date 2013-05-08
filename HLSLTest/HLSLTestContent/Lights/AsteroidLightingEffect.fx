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

    return output;
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
	color += float3(0.1f, 0.1f, 0.1f);

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


	// Calculate final color
	float3 output = saturate(lighting) * color;

	return float4(output, 1);
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
