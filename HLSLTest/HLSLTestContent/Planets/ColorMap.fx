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
	//float3 Normal : TEXCOORD2;
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
	
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 output = float4(0,0,0,0);
	
	/*float3 LightDir = normalize(input.Light);
    float Diffuse = saturate(dot(LightDir, normalize(input.Normal)));*/
	float3 Normal = (2 * (tex2D(BumpMapSampler, input.TexCoord))) - 1.0;
	float3 LightDir = normalize(input.Light);
	float Diffuse = saturate(dot(LightDir, Normal));


	if (renderType == 0) {
		float4 height = tex2D(ColorMapSampler, input.TexCoord);
		float4 texCol = tex2D(PallSampler, float2(subtype, height.x));
		texCol *= Diffuse;
		output =  AmbientColor + texCol;
	} else if (renderType == 1)  {
		// trying multi-texturing
		float4 BaseColor = tex2D(BaseMapSampler, input.TexCoord);
		float4 weightMap = tex2D(WeightMapSampler, input.TexCoord);
		float4 gTex = tex2D(GTextureSampler, input.TexCoord * TextureTiling);// TTï™à¯Ç´êLÇŒÇ≥ÇÍÇÈÅH
		float4 bTex = tex2D(BTextureSampler, input.TexCoord * TextureTiling);
	
		//output = clamp(1.0f - weightMap.r - weightMap.g, 0, 1);
		//output *= BaseColor;
		//output += weightMap.r * rTex + weightMap.g * gTex + weightMap.b * bTex;
		output = clamp(1.0f - weightMap.r - weightMap.g - weightMap.b, 0, 1);
		output += weightMap.r * BaseColor + weightMap.g * gTex+ weightMap.b * bTex;


		float4 height = tex2D(ColorMapSampler, input.TexCoord);
		//float4 texCol = tex2D(PallSampler, float2(0,height.x));
		float4 texCol = tex2D(PallSampler, float2(subtype, height.x));
		texCol *= Diffuse;
		//output =  AmbientColor + texCol;
		output += AmbientColor;
	} else if (renderType == 2) {
		// trying multi-texturing
		float4 BaseColor = tex2D(BaseMapSampler, input.TexCoord);
		float4 weightMap = tex2D(WeightMapSampler, input.TexCoord);

		float3 NormalTex = (2 * (tex2D(BaseNormalSampler, input.TexCoord))) - 1.0;
		float DiffuseTex = saturate(dot(LightDir, Normal));// âˆÇµÇ¢
		BaseColor *= DiffuseTex;

		float4 height = tex2D(ColorMapSampler, input.TexCoord);
		float4 texCol = tex2D(PallSampler, float2(subtype, height.x));
		texCol *= Diffuse;
		//output+= texCol;

		//output = clamp(1.0f - weightMap.r - weightMap.g, 0, 1);

		//output += weightMap.r * BaseColor + weightMap.g * texCol;
		output += weightMap.r * BaseColor * texCol + weightMap.g * texCol;


		output += AmbientColor;
	}


    return output;
}

technique Technique1
{
    pass Pass1
    {

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}