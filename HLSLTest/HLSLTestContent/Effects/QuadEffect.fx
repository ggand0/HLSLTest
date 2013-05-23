float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 wvp;

texture colorMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
texture Mask;
sampler MaskSampler = sampler_state
{
    Texture = (Mask);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};


float Depth;
float FarPlane;
float3 Position;

struct VertexShaderInput
{
    float3 Position : POSITION0;
	//float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0_centroid;
	float4 PositionAnother : TEXCOORD1;
};

float2 halfPixel;
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    input.Position.x =  input.Position.x - 2*halfPixel.x;
    input.Position.y =  input.Position.y + 2*halfPixel.y;/**/
	

    output.Position = float4(input.Position, 1);
	//output.Position = mul(input.Position, View);
	//output.Position = float4(input.Position.xy, Depth, Depth);

	float4 Pos = mul(Position, wvp);
	//output.Position = float4(input.Position.xy, Pos.zw);
	//output.Position = float4(input.Position.xy, Depth, 1);
	//output.Position = float4(input.Position.xyz, Pos.w);// ok
	//output.Position = float4(input.Position.xy, Pos.zw);// 

    output.TexCoord = input.TexCoord;
	output.PositionAnother = Pos;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 process = tex2D(colorSampler, input.TexCoord);
	float4 mask = tex2D(MaskSampler, input.TexCoord);
	//if (mask == float4(0,0,0,1)) {
	if (mask.r == 0 && mask.g == 0 && mask.b == 0) {
		return float4(0,0,0,0);
		//clip();
	} else {
		return process;
	}
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
