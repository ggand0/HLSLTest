//*****************************************************************************//
//*																			  *//
//*****************************************************************************//
float adense=10;
float xpix;
float ypix;
float tpr;
float gridsize=1/512;

#define SAMPLE_COUNT 8
float2 SampleOffsets[SAMPLE_COUNT];

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
	AddressV	= CLAMP;
};

texture FrictionMap;
sampler FrictionMapSampler = sampler_state 
{
    texture = <FrictionMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= WRAP;  
	AddressV	= CLAMP;
};
//*****************************************************************************//
//*																			  *//
//*****************************************************************************//

float4 PixelShaderFunction(float2 texCoord : TEXCOORD) : COLOR0
{
	float4 result=float4(0,0,0,1);

	// atmospheric pressure at the current point
	float4 here = tex2D(HeightMapSampler,texCoord);
	float2 uv = float2(0,0);
	
	for (int i = 0; i < SAMPLE_COUNT; i++)
    {
		// for all the points around the current point, 
		// calculate the pressure difference and use that as a force
		
		float2 samplePoint=texCoord + SampleOffsets[i];
		float4 localPressure=tex2D(HeightMapSampler,samplePoint);
		float pressureDifference=localPressure.x-here.x;			// -1 to +1
 		uv+=pressureDifference*SampleOffsets[i];					// -gridsize to +gridsize
    }
	uv*=200000;
	// estimate the friction force
	float4 friction = tex2D(FrictionMapSampler,texCoord);			// 0 to 1
	float fl =length(uv);
	float fx = (fl*fl*friction.y)+(fl*0.25*friction.y);
	uv.x-=fx/128;
	uv.y-=fx/128;
		
	//uv.x -= 0.2*friction.y*uv.x;
	//estimate the coriolis force
	uv.y+=(uv.x-uv.y)*0.5;
	
	//uv=normalize(uv);												// 0 to 1
	
	
	
	uv/=2;
	uv+=0.5;

	result.xy=uv;
	
	result.w=1;
	return result;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
