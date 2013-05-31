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
float4 LaserColor = float4(1,1,1,1);
float MAX_LENGTH = 120;
bool AdjustedWidth = false;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Direction : TEXCOORD1;
	float Rotation : COLOR0;
	float Speed : TEXCOORD2;
	float StartTime : TEXCOORD3;
	float4 DirectedPosition : POSITION1;
	float Id : TEXCOORD4;
	float Right : TEXCOORD5;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float4 PositionCopy : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float3 position = input.Position;

	float3 startPos = input.Position.xyz / input.Position.w;
	float3 endPos = input.DirectedPosition.xyz / input.DirectedPosition.w;
	float3 lineDir = normalize( endPos - startPos );
	float3 pvUp = normalize(cross(lineDir , cross(Up, Side)));

	float3 size = endPos - startPos;
	float3 leng = sqrt(size.x * size.x + size.y * size.y + size.z * size.z);

	
	//float offset = (input.Right == 1.0f) ? 1 : -1;
	//float offset = (input.Right - 1.0f <= 0.1f) ? 1 : -1;
	float offset = -(input.UV.y - 0.5f) * 2.0f;//input.UV.y * 2 - 1;

	//position += offset.x * leng.x * lineDir + offset.y * Size.y * pvUp;

	// 前の2頂点は既にstripの後ろに割り当てられているはずなので、endPosを左右に振り分けてpositionを決定
	float width = input.Id / MAX_LENGTH;

	//position = endPos + offset * Size.y * pvUp;
	if (AdjustedWidth) {
		position = input.DirectedPosition + offset * width * Size.y * pvUp;
	} else {
		position = input.DirectedPosition + offset * Size.y * pvUp;
	}
	


	// Transform the position by view and projection
	output.Position = mul(float4(position, 1), mul(View, Projection));

	// UV座標はC#側で予め正しい値を設定しておくのでここでは何もしない
	output.UV = input.UV;
	//output.Normal = mul(input.Normal, 1);
	output.PositionCopy = output.Position;

	return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(texSampler, input.UV);

	if (AlphaTest) {
		// discard pixels below a certain transparency threshold:
		clip((color.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));
	}/**/

	return color * LaserColor;
	//return color;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}