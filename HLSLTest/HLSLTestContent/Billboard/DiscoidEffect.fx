float4x4 World;
float4x4 View;
float4x4 Projection;


texture Texture;
sampler TextureSampler = sampler_state {
	texture = <Texture>;

	MinFilter = Anisotropic;// 異方性フィルタ
	MagFilter = Anisotropic;
	MipFilter = Linear;		// Mip-mapping
	AddressU = Wrap;
	AddressV = Wrap;
};
bool TextureEnabled = false;

float3 CameraPosition;
float4 Color;



struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
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

	output.UV = input.UV;// テクスチャを貼るだけなので場所は変えない
	output.ViewDirection = worldPosition - CameraPosition;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Start with diffuse color
	//float3 color = DiffuseColor;
	float4 color = 1;

	// Texture if necessary
	if (TextureEnabled) {
		// 対応するピクセルのテクスチャの色を持ってくるためにtex2D関数を使用
		color *= tex2D(TextureSampler, input.UV);
	}

	

	//return float4(output, 1);
	return color * Color;
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
