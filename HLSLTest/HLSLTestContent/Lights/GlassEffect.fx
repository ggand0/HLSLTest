float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 matInverseWorld;

float4 vecEye;

texture RefractionMap;
sampler2D RefractionSampler = sampler_state {
	texture = <RefractionMap>;
	//MinFilter = Anisotropic;
	//MagFilter = Anisotropic;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Mirror;
	AddressV = Mirror;
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
	float2 UV : TEXCOORD4;
	float4 PositionCopy : TEXCOORD5;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	
	//Out.Pos = mul(Pos, matWorldViewProj);
	output.Normal = normalize(mul(matInverseWorld, input.Normal));
	//output.Normal = mul(normalize(input.Normal), World);// bug‚Á‚Ä–Ê”’‚¢
	output.ViewDirection = normalize(vecEye - output.Position);
	output.L = LightDirection;
	output.UV = input.UV;
	output.PositionCopy = output.Position;

    return output;
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	/*float2 reflectionUV = postProjToScreen(input.ReflectionPosition) + halfPixel();
	//float3 reflection = tex2D(reflectionSampler, reflectionUV);
	float3 BaseColor = float3(0.2, 0.2, 0.8);
	float BaseColorAmount = 0.3f;
	float4 normal = tex2D(waterNormalSampler, input.NormalMapPosition) * 2 - 1;
	float2 UVOffset = WaveHeight * normal.rg;
	float3 reflection = tex2D(reflectionSampler, reflectionUV + UVOffset);
	float3 viewDirection = normalize(CameraPosition - input.WorldPosition);
	float3 reflectionVector = -reflect(LightDirection, normal.rgb);
	float specular = dot(normalize(reflectionVector), viewDirection);
	specular = pow(specular, 256);*/


	// Calculate normal diffuse light.
	float4 Color = float4(1,1,1,1);//tex2D(ColorMapSampler, Tex);
	float Diff = saturate(dot(input.L, input.Normal));

	float3 ViewDir = normalize(input.ViewDirection);
    //float3 Refract = refract(ViewDir, input.Normal, 0.66);
	float3 Refract = refract(ViewDir, input.Normal, 0.99);

	float2 refractionUV = postProjToScreen(input.PositionCopy) + halfPixel();
    //float3 RefractColor = tex2D(RefractionSampler, Refract);
	float3 RefractColor = tex2D(RefractionSampler, refractionUV);
 
    // return the color
	// return Color * vAmbient*float4(ReflectColor,1) + Color * vDiffuseColor * Diff*float4(ReflectColor,1) + vSpecularColor * Specular;
    return float4(RefractColor, 1) * tint;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
