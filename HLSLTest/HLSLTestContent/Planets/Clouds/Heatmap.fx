
float esun=10;

texture HeightMap;
sampler HeightMapSampler = sampler_state 
{
    texture = <HeightMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= CLAMP;  
	AddressV	= CLAMP;
};


float4 PixelShaderFunction(float2 texCoord : TEXCOORD) : COLOR0
{
	float4 result=float4(0,0,0,1);
	float4 land = tex2D(HeightMapSampler,texCoord);
	if (land.r<=0.001)
	{
		land.r=0.5;
	}
	float  angle=3.141597*texCoord.y;
	float  intensity=sin(angle);
	result.r=(((1-land.r)*intensity*intensity)+(intensity/2))*esun;
	result.a=1;
	return result;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
