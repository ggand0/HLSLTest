float4x4 World;
float4x4 View;
float4x4 Projection;

texture BasicTexture;
sampler BasicTextureSampler = sampler_state {
texture = <BasicTexture>;
};
bool TextureEnabled = true;

float3 AmbientLightColor = float3(.15, .15, .15);
float3 DiffuseColor = float3(.85, .85, .85);
float3 LightPosition = float3(0, 5000, 0);
float3 LightDirection = float3(0, -1, 0);
float ConeAngle = 90;
float3 LightColor = float3(1, 1, 1);
float LightFalloff = 20;

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
	float4 WorldPosition : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // TODO: ここで頂点シェーダー コードを追加します。
	output.WorldPosition = worldPosition;
	output.UV = input.UV;
	output.Normal = mul(input.Normal, World);// 法線もTransformさせておく

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 diffuseColor = DiffuseColor;
	if (TextureEnabled)
		diffuseColor *= tex2D(BasicTextureSampler, input.UV).rgb;
	float3 totalLight = float3(0, 0, 0);
	totalLight += AmbientLightColor;

	float3 lightDir = normalize(LightPosition - input.WorldPosition);// 点光源への方向ベクトル
	// 光の拡散を計算：ここはLambert lightingと同じ？
	float diffuse = saturate(dot(normalize(input.Normal), lightDir));// saturateはMath.Clampと同様
	
	// (dot(p - lp, ld) / cos(a))^f
	float d = dot(-lightDir, normalize(LightDirection));
	float a = cos(ConeAngle);
	float att = 0;
	if (a < d) {
		att = 1 - pow(clamp(a / d, 0, 1), LightFalloff);
	}

	totalLight += diffuse * att * LightColor;
	return float4(diffuseColor * totalLight, 1);
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
