// ver2 : refraction mapをCubeMapに変更。
// Tex2Dに落としてから色々調整してサンプルするのではなく、先にCubeMapにObjectを全て描画して動的に作成してからこちらで普通に読ませるようにする

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 matInverseWorld;

float4 vecEye;

texture RefractionMap;
samplerCUBE RefractionSampler = sampler_state {
	texture = <RefractionMap>;
};

float3 RefractColor;
float3 LightDirection = float3(1, 1, 1);
float4 tint = float4(1,1,1,1);//And the structures look like this
#include "..\\PPShared.vsi"

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 Tex : TEXCOORD0;
	float3 Normal : NORMAL0;
	float2 UV : TEXCOORD0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 L : TEXCOORD1;
	float3 Normal : TEXCOORD2;
	float3 ViewDirection : TEXCOORD3;
	float4 PositionCopy : TEXCOORD5;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    /*float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);*/
	
	output.Position = mul(worldPosition, mul(View, Projection));
	//output.Normal = mul(input.Normal, World);
	output.PositionCopy = output.Position;

	output.Normal = normalize(mul(matInverseWorld, input.Normal));
	//output.ViewDirection = normalize(vecEye - output.Position);

	float3 PosWorldr = (mul(input.Position, World));
	float3 ViewDir = normalize(PosWorldr - vecEye);
	//float3 ViewDir = normalize(vecEye - PosWorldr);
	output.ViewDirection = ViewDir;
	output.L = LightDirection;
	
    return output;
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Calculate normal diffuse light.
	float4 Color = float4(1,1,1,1);//tex2D(ColorMapSampler, Tex);
	float Diff = saturate(dot(input.L, input.Normal));


	//float3 Refract = refract(input.ViewDirection, input.Normal, 0.99);
	float3 Refract = refract(input.ViewDirection, input.Normal, 0.2);
	float3 RefractColor0 = texCUBE(RefractionSampler, Refract);

	/*float2 refractionUV = postProjToScreen(input.PositionCopy) + halfPixel();
    //float3 RefractColor = tex2D(RefractionSampler, Refract);
	float3 RefractColor = tex2D(RefractionSampler, refractionUV);*/
	
 
    // return the color
	// return Color * vAmbient*float4(ReflectColor,1) + Color * vDiffuseColor * Diff*float4(ReflectColor,1) + vSpecularColor * Specular;
    return float4(RefractColor0, 1) * tint;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
