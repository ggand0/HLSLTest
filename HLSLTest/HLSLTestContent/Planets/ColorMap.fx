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
    
    output.Position = mul(input.Position,wvp);	
	//output.Light = LightDirection;
	output.TexCoord = input.TexCoord;
	// output.Normal = mul(input.Normal,world);

	 
	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(input.Tangent,world);
	worldToTangentSpace[1] = mul(cross(input.Tangent,input.Normal),world);
	worldToTangentSpace[2] = mul(input.Normal,world);
	
	output.Light = mul(worldToTangentSpace,LightDirection);	
	
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 output = float4(0,0,0,0);
	
	/*float3 LightDir = normalize(input.Light);
    float Diffuse = saturate(dot(LightDir,normalize(input.Normal)));*/

	float3 Normal = (2 * (tex2D(BumpMapSampler,input.TexCoord))) - 1.0;
	float3 LightDir = normalize(input.Light);
	float Diffuse = saturate(dot(LightDir,Normal));
		
	float4 height = tex2D(ColorMapSampler, input.TexCoord);
	//float4 texCol = tex2D(PallSampler,float2(0,height.x));
	float4 texCol = tex2D(PallSampler, float2(subtype, height.x));

	texCol *= Diffuse;
	
	output =  AmbientColor + texCol;
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