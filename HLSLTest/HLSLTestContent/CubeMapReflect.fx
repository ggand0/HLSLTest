float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

texture CubeMap;
samplerCUBE CubeMapSampler = sampler_state {
	texture = <CubeMap>;
	minfilter = anisotropic;
	magfilter = anisotropic;
};

float4 ClipPlane;
bool ClipPlaneEnabled = false;


struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 WorldPosition : TEXCOORD0;
	float3 Normal : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
	output.WorldPosition = worldPosition;

	output.Position = mul(worldPosition, mul(View, Projection));
	output.Normal = mul(input.Normal, World);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled) {
		clip(dot(float4(input.WorldPosition, 1), ClipPlane));
	}
	float3 viewDirection = normalize(input.WorldPosition - CameraPosition);
	float3 normal = normalize(input.Normal);

	// Reflect around normal
	float3 reflection = reflect(viewDirection, normal);
	return texCUBE(CubeMapSampler, reflection);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
