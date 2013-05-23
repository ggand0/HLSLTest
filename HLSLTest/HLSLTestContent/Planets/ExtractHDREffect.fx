sampler InputSampler : register( s0 );

texture2D GradientTexture;

sampler GradientTextureSampler = sampler_state
{
	Texture = <GradientTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap; 
};

float Threshold;
float Brightness;

float Luminance( float4 Color )
{
	return 
		0.3 * Color.r +
		0.6 * Color.g +
		0.1 * Color.b;
}

float4 PS_ExtractHDR( 
	float2 inTex: TEXCOORD0,
	uniform float Threshold,
	uniform float Brightness ) : COLOR0
{	
	float4 color = tex2D( InputSampler, inTex );
	float lum = Luminance( color );

	if (lum >= Threshold) {
		float pos = lum - Threshold;
		if( pos > 0.98 ) pos = 0.98;
		if( pos < 0.02 ) pos = 0.02;
		color = tex2D( GradientTextureSampler, pos ) * Brightness;
		return float4( color.rgb, 1.0 );
	} else {
		return float4( 0.0, 0.0, 0.0, 0.0 );// アルファが含まれないのはここのせいでしたサーセン
	}
} 

technique Main
{
    pass p0
	{		
		PixelShader = compile ps_2_0 PS_ExtractHDR(
			Threshold,
			Brightness );	
    }
}