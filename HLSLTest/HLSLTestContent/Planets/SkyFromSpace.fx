float4x4 World;
float4x4 View;
float4x4 Projection;

float3 v3LightPos;			// The camera's current position
float3 v3CameraPos;			// The camera's current position
float3 v3LightDir;			// Direction vector to the light source
float3 v3InvWavelength;		// 1 / pow(wavelength, 4) for RGB
float fCameraHeight;		// The camera's current height
float fCameraHeight2;		// fCameraHeight^2
float fOuterRadius;			// The outer (atmosphere) radius
float fOuterRadius2;		// fOuterRadius^2
float fInnerRadius;			// The inner (planetary) radius
float fInnerRadius2;		// fInnerRadius^2
float fKrESun;				// Kr * ESun
float fKmESun;				// Km * ESun
float fKr4PI;				// Kr * 4 * PI
float fKm4PI;				// Km * 4 * PI
float fScale;				// 1 / (fOuterRadius - fInnerRadius)
float fScaleOverScaleDepth;	// fScale / fScaleDepth
float fScaleDepth;			// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
float fSamples;
int nSamples;

float g =-0.90f;
float g2 = 0.81f;
float fExposure =2;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color0   : COLOR0;
	float4 Color1   : COLOR1;
	float3 v3Direction : COLOR2;
};

float scale(float fCos)
{
	float x = 1.0 - fCos;  
	return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    

	float3 v3Pos = worldPosition.xyz;
	float3 v3Ray = v3Pos - v3CameraPos;
	float fFar = length(v3Ray);

	v3Ray /= fFar;

	// Calculate the closest intersection of the ray with the outer atmosphere 
	// (which is the near point of the ray passing through the atmosphere)
	
	float B = 2.0 * dot(v3CameraPos, v3Ray);
	float C = fCameraHeight2 - fOuterRadius2;
	float fDet = max(0.0, B*B - 4.0 * C);
	float fNear = 0.5 * (-B - sqrt(fDet));

	// Calculate the ray's start and end positions in the atmosphere,
	// then calculate its scattering offset

	float3 v3Start = v3CameraPos + v3Ray * fNear;
	fFar -= fNear;
	//float fStartAngle = dot(v3Ray, v3Start) / fOuterRadius;
	//float fStartDepth = exp(-1.0 / fScaleDepth);
	//float fStartOffset = fStartDepth * scale(fStartAngle);


	// Initialize the scattering loop variables
	float fSampleLength = fFar / fSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Ray * fSampleLength;
	float3 v3SamplePoint = v3Start + (v3SampleRay * 0.5);
	
	float fHeight = length(v3SamplePoint);
	float fStartAngle = dot(v3Ray, v3Start) / fHeight;
	float fStartDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight)); 
	//float fStartDepth = exp((fInnerRadius - fOuterRadius) * fScaleOverScaleDepth);
	float fStartOffset = fStartDepth * scale(fStartAngle);
	
	// Now loop through the sample points

	float3 v3FrontColor = float3(0.0, 0.0, 0.0);
	for(int i=0; i<nSamples; i++) 
	{
		float fHeight = length(v3SamplePoint);
		float fDepth = exp(fScaleOverScaleDepth * (fHeight-fOuterRadius));//(fInnerRadius - fHeight)); //
		float fLightAngle = dot(v3LightDir, v3SamplePoint) / fHeight;
		float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
		float fScatter = (fStartOffset + fDepth * (scale(fLightAngle) - scale(fCameraAngle)));
		float3 v3Attenuate = exp(-fScatter *(v3InvWavelength * fKr4PI + fKm4PI));
		v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
		v3SamplePoint += v3SampleRay;
	}

	output.Position = mul(viewPosition, Projection);
	output.Color0 = float4(v3FrontColor * v3InvWavelength * fKrESun, 1);
	output.Color1 = float4(v3FrontColor * fKmESun, 1);
	output.v3Direction = v3CameraPos - v3Pos;
    return output;
}

// Mie phase function
float phaseFunctionM(float mu) 
{
	return 1.5 * 1.0 / fKm4PI * (1.0 - g2) * pow(1.0 + (g2) - 2.0*g*mu, -3.0/2.0) * (1.0 + mu * mu) / (2.0 + g2);
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	
    float fCos = dot(v3LightDir, input.v3Direction) / length(input.v3Direction);
	float fRayleighPhase = 1.6 * (1.0 + fCos*fCos);
	float fMiePhase = phaseFunctionM(fCos);
	float4 col=(fRayleighPhase * input.Color0) + (fMiePhase * input.Color1);
	col.a=max(col.b,col.r);
	//return col;
	
	return 1 - exp(-fExposure * col);
}	
	

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
