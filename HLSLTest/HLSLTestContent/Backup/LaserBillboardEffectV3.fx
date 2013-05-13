float4x4 World;
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

float4 StartPos;
float4 EndPos;
float theta;
float3 ProjectedVector;// End - Startをスクリーン平面に射影したベクトル
float3 CameraDir;
// 流石にこれは予め計算させておいたほうがいいか→関数化で
//float2x2 rotate = { {cos(theta), -sin(theta)}, { sin(theta), cos(theta) } };

float3 CenterNormal;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	//float3 Normal : NORMAL0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	//float3 Normal : TEXCOORD1;
	float4 PositionCopy : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float3 position = input.Position;

	// Determine which corner of the rectangle this vertex represents
	float2 offset = float2((input.UV.x - 0.5f) * 2.0f, -(input.UV.y - 0.5f) * 2.0f);

	// Move the vertex along the camera's 'plane' to its corner
	float3 pvUp = normalize(cross(ProjectedVector, cross(Up, Side)));
	position += offset.x * Size.x * ProjectedVector + offset.y * Size.y * pvUp;


	// Transform the position by view and projection
	output.Position = mul(float4(position, 1), mul(View, Projection));
	output.UV = input.UV;
	//output.Normal = mul(input.Normal, 1);
	output.PositionCopy = output.Position;

	return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(texSampler, input.UV);

	/*// determines whether we're processing at the center (1) or the edges (near 0)
    float cosang = 1 - abs((dot(CenterNormal, input.Normal)));
	// bit of alpha blending on the ends
    float endfade = clamp(1 - (abs(input.PositionCopy.z) + .05), 0, 1);
    // falloff controlled by the exponent
    endfade =  pow(endfade, .075);
	//alpha on the edges.. again, fadeoff controlled by the exponent
    color.a = pow(abs(cosang), 1.25);
	color.a = min(color.a, endfade);*/


	if (AlphaTest) {
		// discard pixels below a certain transparency threshold:
		clip((color.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));
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