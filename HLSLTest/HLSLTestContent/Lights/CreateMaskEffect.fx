float4x4 wvp;
float3 colour;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;

};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = mul(input.Position,wvp);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return float4(colour, 1);
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
