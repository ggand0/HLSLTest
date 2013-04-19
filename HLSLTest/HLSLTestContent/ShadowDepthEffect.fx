float4x4 World;
float4x4 View;
float4x4 Projection;

float FarPlane = 10000.0f;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 ScreenPosition : TEXCOORD0;// つまりDepth?
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	// Calculate the screen space position
    float4x4 wvp = mul(World, mul(View, Projection));	
	float4 position = mul(input.Position, wvp);// スクリーン座標計算

    output.Position = position;
	output.ScreenPosition = position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Determine the depth of this vertex / by the far plane distance,
	// limited to [0, 1]
	float depth = clamp(input.ScreenPosition.z / FarPlane, 0, 1);
	//float depth = clamp(input.ScreenPosition.z / 2000.0, 0, 1);
	//float depth = clamp(input.ScreenPosition.z / input.ScreenPosition.w, 0, 1);

	// Return only the depth value
	//return float4(depth, 0, 0, 1);
	return float4(depth, depth, depth, 1);

	//float d = input.Position.z;
	//return float4(d, d, d, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: ここでレンダーステートを設定します。

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
