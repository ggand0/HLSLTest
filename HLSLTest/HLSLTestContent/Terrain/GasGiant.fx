#define ONE 0.00390625
#define ONEHALF 0.001953125
#include "..\\SimplexNoise.vsi"

float xOvercast=1.1;

texture ColorMap;
sampler ColorMapSampler = sampler_state 
{
    texture = <ColorMap>;    
    magfilter	= POINT; 
	minfilter	= POINT; 
	mipfilter	= POINT; 
	AddressU	= WRAP;  
	AddressV	= WRAP;
};

//======================================================================================
//=
//=
//=
//======================================================================================
struct VS_IN
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
};
struct VS_OUT
{
	float4 Position : POSITION;
	float3 WP : TEXCOORD3;
};

struct PS_OUT
{
	float4 Color : COLOR;
};

//======================================================================================
//=
//=
//=
//======================================================================================

VS_OUT VS_ColorMap(VS_IN input)
{
	VS_OUT output = (VS_OUT)0;
	
	output.Position.x=(input.TexCoord.x*2)-1;
	output.Position.y=(input.TexCoord.y*2)-1;
	output.Position.z=1;
	output.Position.w=1;
	
	output.WP = input.Position;
	return output;
}

float4 PS_ColorMap(VS_OUT input) : COLOR0
{
	/*PS_OUT output = (PS_OUT)0;
	
	float amp=2;
	float freq=1;
	float y=0;
	for (int i=0; i<4; i++)
	{
	   freq = (2 * pow(2 ,i)) - 1;
	   //amp=pow(0.5, i);
	   amp=pow(0.25, i);
	   y += snoise(ColorMapSampler, input.WP * freq) * amp;
	}
	
	y = (y + 1) / 2;
	y = 1.0f-pow(y, xOvercast) * 2.0f;

	output.Color =  float4(y.x ,y.x ,y.x ,1);
	return output;*/
	 float res=snoise(ColorMapSampler, input.WP*6);
    float3 dr = float3(2,res+10,2);
    float h = snoise(ColorMapSampler, input.WP*dr);
    float4 texCol = float4(h,h,h,1);

    return texCol;
}

technique technique1
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VS_ColorMap();
		PixelShader = compile ps_3_0 PS_ColorMap();
	}
}