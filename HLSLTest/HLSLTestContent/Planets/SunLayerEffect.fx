float4x4 wvp;
float4x4 WorldIT;
float3 colour;

texture BaseTexture;
samplerCUBE BaseTextureSampler = sampler_state
{
	texture = <BaseTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal	: NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Normal	: TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = mul(input.Position,wvp);
	output.Normal = mul(input.Normal, WorldIT);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 normal = normalize(input.Normal);
	float4 color = texCUBE(BaseTextureSampler, normal.xyz);
    return float4(color.xyz, 1);
}

technique Technique1
{
    pass Pass1
    {
       

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
