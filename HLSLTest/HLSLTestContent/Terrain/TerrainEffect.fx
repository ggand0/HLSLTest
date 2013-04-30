float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightDirection = float3(1, -1, 0);
float TextureTiling = 1;

texture2D BaseTexture;
sampler2D BaseTextureSampler = sampler_state {
	Texture = <BaseTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};
texture WeightMap;
sampler WeightMapSampler = sampler_state {
	texture = <WeightMap>;
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
};

texture RTexture;
sampler RTextureSampler = sampler_state
{
	texture = <RTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};
texture GTexture;
sampler GTextureSampler = sampler_state
{
	texture = <GTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
}

texture BTexture;
sampler BTextureSampler = sampler_state
{
	texture = <BTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

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
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, mul(View, Projection));
	output.Normal = input.Normal;
	output.UV = input.UV;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float light = dot(normalize(input.Normal),
		normalize(LightDirection));
	light = clamp(light + 0.4f, 0, 1); // Simple ambient lighting


	float3 rTex = tex2D(RTextureSampler, input.UV * TextureTiling);
	float3 gTex = tex2D(GTextureSampler, input.UV * TextureTiling);
	float3 bTex = tex2D(BTextureSampler, input.UV * TextureTiling);
	//float3 tex = tex2D(BaseTextureSampler, input.UV * TextureTiling);
	//return float4(tex * light, 1);

	float3 base = tex2D(BaseTextureSampler, input.UV * TextureTiling);
	float3 weightMap = tex2D(WeightMapSampler, input.UV);
	float3 output = clamp(1.0f - weightMap.r - weightMap.g -
	weightMap.b, 0, 1);
	output *= base;
	output += weightMap.r * rTex + weightMap.g * gTex +
	weightMap.b * bTex;

	return float4(output * light, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
