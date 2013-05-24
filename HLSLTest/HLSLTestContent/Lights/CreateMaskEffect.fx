float4x4 wvp;
float3 colour;

float DepthValue;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 PositionCopy : TEXCOORD0;
	float Depth : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = mul(input.Position,wvp);
	output.PositionCopy = output.Position;
	output.Depth = output.Position.z;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float d = DepthValue;

	//if (input.Depth > d) {
	if (abs(input.Depth) < abs(d)) {
		return float4(colour, 1);
	} else {
		return float4(1,1,1,1);
	}

	//return float4(colour, 1);
    //return float4(0,0,0, 1);
}

technique Technique1
{
    pass Pass1
    {
       

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
