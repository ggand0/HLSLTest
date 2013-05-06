//*****************************************************************************//
//*																			  *//
//*****************************************************************************//
float3 LightDirection : Direction = float3(1,0,1);
float4x4 wvp : WorldViewProjection;
float4x4 world : World;
float4 AmbientColor : AMBIENT = float4(0.1,0.1,0.1,1);
float4 CloudColour=float4(1,1,1,1);

//*****************************************************************************//
//*																			  *//
//*****************************************************************************//
texture HeightMap;
sampler HeightMapSampler = sampler_state 
{
    texture = <HeightMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= WRAP;  
	AddressV	= WRAP;
};

texture CloudMap;
sampler CloudMapSampler = sampler_state 
{
    texture = <CloudMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= WRAP;  
	AddressV	= WRAP;
};

texture CloudBumpMap;
sampler CloudBumpMapSampler = sampler_state 
{
    texture = <CloudBumpMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= WRAP;  
	AddressV	= WRAP;
};

texture PaletteMap;
sampler PaletteMapSampler = sampler_state 
{
    texture = <PaletteMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= CLAMP;  
	AddressV	= CLAMP;
};

//*****************************************************************************//
//*																			  *//
//*****************************************************************************//
struct VertexShaderInput
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT;
};

struct VertexShaderOutput
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 Light : TEXCOORD1;
	float3 Normal : TEXCOORD2;	
};

//*****************************************************************************//
//*																			  *//
//*****************************************************************************//
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
   VertexShaderOutput output = (VertexShaderOutput)0;
	
	output.Position = mul(input.Position,wvp);
	
	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(input.Tangent,world);
	worldToTangentSpace[1] = mul(cross(input.Tangent,input.Normal),world);
	worldToTangentSpace[2] = mul(input.Normal,world);
	
	output.Light = mul(worldToTangentSpace,LightDirection);	
	
	output.TexCoord = input.TexCoord;
	//output.TexCoord.x=1-output.TexCoord.x;
	
	return output;
}
//*****************************************************************************//
//*																			  *//
//*****************************************************************************//
float4 PixelShaderFunction(float2 texCoord : TEXCOORD) : COLOR0
{
	float4 result=float4(0,0,0,1);
	float4 land = tex2D(HeightMapSampler,texCoord);
	float4 cland = tex2D(PaletteMapSampler,float2(0,land.x));
	float4 cloud = tex2D(CloudMapSampler,texCoord);
	
	float cl = cloud.x *4;
	float4 clouds=float4(cl,cl,cl,1);
	
	result=lerp(cland,clouds,cl);
	result.a=1;
	return result;
}
//*****************************************************************************//
//*																			  *//
//*****************************************************************************//
float4 BumpShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 result;
	float4 cloud = tex2D(CloudMapSampler,input.TexCoord);
	
	float cl = cloud.x*4;
	float4 clouds=CloudColour*cl;
	float3 Normal = (2 * (tex2D(CloudBumpMapSampler,input.TexCoord))) - 1.0;
	float3 LightDir = normalize(input.Light);
	float Diffuse = saturate(dot(LightDir,Normal));
	clouds*=Diffuse;
	result=clouds;
	result.a=cl;
	return result;
}
//*****************************************************************************//
//*																			  *//
//*****************************************************************************//
float4 PlanetBumpShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 result;
	float4 land = tex2D(HeightMapSampler,input.TexCoord);
	float4 cland = tex2D(PaletteMapSampler,float2(0,land.x));
	
	float3 Normal = (2 * (tex2D(CloudBumpMapSampler,input.TexCoord))) - 1.0;
	float3 LightDir = normalize(input.Light);
	float Diffuse = saturate(dot(LightDir,Normal));
	Diffuse+=0.1;
	cland*=Diffuse;
	
	result=cland;
	result.a=1;
	return result;
}
//*****************************************************************************//
//*																			  *//
//*****************************************************************************//
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

technique Bumped
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 BumpShaderFunction();
    }
}
technique BumpedPlanet
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PlanetBumpShaderFunction();
    }
}
