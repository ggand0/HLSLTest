float4x4 View;
float4x4 Projection;
texture ParticleTexture;
sampler2D texSampler = sampler_state {
	texture = <ParticleTexture>;
};
float2 Size;
float3 Up; // Camera's 'up' vector
float3 Side; // Camera's 'side' vector

bool AlphaTest = true;
bool AlphaTestGreater = true;
float AlphaTestValue = 0.5f;

float Offset = 10.0f;// どれだけの距離までを透過するか
texture DepthMap;
sampler DepthSampler = sampler_state
{
	texture = <DepthMap>;
	//MagFilter = Linear;
	MagFilter = Point;
	MipFilter = Point;//None;
	AddressU = Clamp;//Border;
	AddressV = Clamp;//Border;
	//BorderColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
};
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float4 Depth : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float3 position = input.Position;
	// Determine which corner of the rectangle this vertex represents
	float2 offset = float2((input.UV.x - 0.5f) * 2.0f, -(input.UV.y - 0.5f) * 2.0f);

	// Move the vertex along the camera's 'plane' to its corner
	position += offset.x * Size.x * Side + offset.y * Size.y * Up;

	// Transform the position by view and projection
	output.Position = mul(float4(position, 1), mul(View, Projection));
	output.UV = input.UV;
	output.Depth = output.Position; // 出力する深度情報の作成

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(texSampler, input.UV);
	if (AlphaTest) {
		// discard pixels below a certain transparency threshold:
		clip((color.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));
	}

	// soft particle test
	input.Depth.xy /= input.Depth.w;
	float2 coord = input.Depth.xy * float2(0.5f, -0.5f) + 0.5f;
	float depth = tex2D(DepthSampler, coord).x;

	float alpha = depth * input.Depth.w - input.Depth.z;
	if (alpha <= Offset) {
		alpha /= Offset;
		color.w *= alpha;
	}

	return color;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}