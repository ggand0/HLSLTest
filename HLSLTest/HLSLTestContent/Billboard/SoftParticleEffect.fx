// 参考：
// http://kayugames.wordpress.com/2012/01/22/%E3%82%BD%E3%83%95%E3%83%88%E3%83%91%E3%83%BC%E3%83%86%E3%82%A3%E3%82%AF%E3%83%AB/

float4x4 View;
float4x4 Projection;
texture ParticleTexture;
sampler2D texSampler = sampler_state {
	texture = <ParticleTexture>;
};
float Time;
float Lifespan;
float2 Size;
float3 Wind;
float3 Up;
float3 Side;
float FadeInTime;

bool AttachColor;
float4 ParticleColor;

float Offset = 1.0f;// どれだけの距離までを透過するか
texture DepthMap;
sampler DepthSampler = sampler_state
{
	texture = <DepthMap>;
	MagFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;//Border;
	AddressV = Clamp;//Border;
	//BorderColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
};
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Direction : TEXCOORD1;
	float Rotation : COLOR0;
	float Speed : TEXCOORD2;
	float StartTime : TEXCOORD3;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float2 RelativeTime : TEXCOORD1;
	float Rotation : COLOR0;
	float4 Depth : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float3 position = input.Position;
	// Move to billboard corner
	float2 offset = Size * float2((input.UV.x - 0.5f) * 2.0f, -(input.UV.y - 0.5f) * 2.0f);
	position += offset.x * Side + offset.y * Up;

	// Determine how long this particle has been alive
	float relativeTime = (Time - input.StartTime);
	output.RelativeTime = relativeTime;

	// Move the vertex along its movement direction and the wind direction
	position += (input.Direction * input.Speed + Wind) * relativeTime;

	// Transform the final position by the view and projection matrices
	output.Position = mul(float4(position, 1), mul(View, Projection));
	output.UV = input.UV;
	output.Rotation = (input.Rotation + 3.14159) / 6.283185;
	output.Depth = output.Position; // 出力する深度情報の作成

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Ignore particles that aren't active
	clip(input.RelativeTime);
	

	float r = (input.Rotation * 6.283185) - 3.141593;
	float c = cos(r);
	float s = sin(r);
	float2x2 rotationMatrix = float2x2(c, -s, s, c);

	float2 texCoord = mul(input.UV - 0.5, rotationMatrix);
	//return tex2D(TextureSampler, texCoord + 0.5) * i.Colour;

	// Sample texture
	//float4 color = tex2D(texSampler, input.UV);
	float4 color = tex2D(texSampler, texCoord + 0.5);

	float d = 1;
	// Fade out towards end of life
	//d = clamp(1.0f - pow((input.RelativeTime / Lifespan), 10), 0, 1);
	// Fade in at beginning of life
	//d *= clamp((input.RelativeTime / FadeInTime), 0, 1);


	// soften the edge
	input.Depth.xy /= input.Depth.w;
	float2 coord = input.Depth.xy * float2( 0.5f, -0.5f ) + 0.5f;
	float depth = tex2D(DepthSampler, coord).x;

	//float alpha = depth * input.Depth.w – input.Depth.z;
	float alpha = depth * input.Depth.w - input.Depth.z;
	if (alpha <= Offset) {
		alpha /= Offset;
		color.w *= alpha;
	}

	// Return color * fade amount
	//return float4(color * d);
	if (AttachColor) {
		return float4(color * ParticleColor * d);
	} else {
		return float4(color * d);
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
