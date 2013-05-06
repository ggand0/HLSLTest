
float cland=10;
float cwater=10;

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

	float friction = lerp(cwater,cland,land.x);
	result.g=friction;
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
