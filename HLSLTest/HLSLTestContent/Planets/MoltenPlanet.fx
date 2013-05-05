float4x4 wvp : WorldViewProjection;
float4x4 world : World;
float AmbientIntensity = 1;
float4 AmbientColor : AMBIENT = float4(0,0,0,1);
float3 LightDirection : Direction = float3(1,0,1);

texture Lava;
sampler LavaSampler = sampler_state 
{
    texture = <Lava>;
};


texture Palette;
sampler PallSampler = sampler_state 
{
    texture = <Palette>;
    AddressU	= CLAMP;  
	AddressV	= CLAMP;
};

texture ColorMap : Diffuse;
sampler ColorMapSampler = sampler_state 
{
    texture = <ColorMap>;
    AddressU	= CLAMP;  
	AddressV	= CLAMP;
};

texture GlowMap : Diffuse;
sampler GlowMapSampler = sampler_state 
{
    texture = <GlowMap>;
};

texture BumpMap ;
sampler BumpMapSampler = sampler_state
{
	Texture = <BumpMap>;
};
float subtype=0;

struct VS_IN
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT;
};
struct VS_OUT
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 Light : TEXCOORD1;
};
struct PS_OUT
{
	float4 Color : COLOR;
};


VS_OUT VS_Molten(VS_IN input)
{
	VS_OUT output = (VS_OUT)0;
	
	output.Position = mul(input.Position,wvp);
	
	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(input.Tangent,world);
	worldToTangentSpace[1] = mul(cross(input.Tangent,input.Normal),world);
	worldToTangentSpace[2] = mul(input.Normal,world);
	
	output.Light = mul(worldToTangentSpace,LightDirection);	
	
	output.TexCoord = input.TexCoord;
	
	return output;
}
float4 PS_Molten(VS_OUT input):Color
{
	float4 output;
	float4 texCol;

	float3 Normal = (2 * (tex2D(BumpMapSampler,input.TexCoord))) - 1.0;
	
	float3 LightDir = normalize(input.Light);
	float Diffuse = saturate(dot(LightDir,Normal));
	
	float4 height = tex2D(ColorMapSampler,input.TexCoord);
	if (height.x==0)
	{
		texCol = tex2D(LavaSampler,input.TexCoord);
		Diffuse*=2;
	}else{
		texCol = tex2D(PallSampler,float2(0,height.x));
	}
	
	texCol *= Diffuse;
	
	output =  AmbientColor + texCol;
    return output;
	
}

technique AmbientLight
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 VS_Molten();
		PixelShader = compile ps_2_0 PS_Molten();
	}
}
