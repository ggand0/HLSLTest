#define ONE 0.00390625
#define ONEHALF 0.001953125
#include "..\\SimplexNoise.vsi"
#include "..\\CellularNoise.vsi"

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

PS_OUT PS_ColorMap(VS_OUT input)
{
	PS_OUT output = (PS_OUT)0;

    float amp=2;
    float freq=1;
    float2 y=float2(0,0);
    for (int i=0; i<2; i++)
    {
        freq = (2*pow(2,i))-1;
        amp=pow(0.7,i);
        y+=gpuCellNoise3D(ColorMapSampler, input.WP*freq).x*amp;
    }

    float h=snoise(ColorMapSampler, input.WP*10);
    float3 dr = float3(h,h*4,h);
    h = snoise(ColorMapSampler, input.WP*dr);
    y*=3;
    y+=(h+1)/3;

    y=(y/3);

    output.Color =  float4(y.x,y.x,y.x,1);  //texCol;
    return output;
}

technique technique1
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VS_ColorMap();
		PixelShader = compile ps_3_0 PS_ColorMap();
	}
}