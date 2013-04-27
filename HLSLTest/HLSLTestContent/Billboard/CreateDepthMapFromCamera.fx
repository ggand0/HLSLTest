// https://kayugames.wordpress.com/2012/01/18/%E3%82%B7%E3%83%A3%E3%83%89%E3%82%A6%E3%83%9E%E3%83%83%E3%83%97-%E3%82%B7%E3%83%A3%E3%83%89%E3%82%A6%E3%83%9E%E3%83%83%E3%83%97%E7%94%BB%E5%83%8F%E4%BD%9C%E6%88%90/
// より

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
	float4 Depth : TEXCOORD0;// 深度
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	// Calculate the screen space position
    float4x4 wvp = mul(World, mul(View, Projection));	
	float4 position = mul(input.Position, wvp);// スクリーン座標計算

    output.Position = position;
	output.Depth = output.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    /*// Determine the depth of this vertex / by the far plane distance,
	// limited to [0, 1]
	float depth = clamp(input.ScreenPosition.z / FarPlane, 0, 1);
	// Return only the depth value
	//return float4(depth, 0, 0, 1);
	return float4(depth, depth, depth, 1);*/

	float depth = ( input.Depth.z / input.Depth.w );
    return depth;
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
