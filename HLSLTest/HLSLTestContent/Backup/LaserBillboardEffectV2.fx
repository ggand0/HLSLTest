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


struct VertexShaderInput
{
	float4 Position : POSITION0;// 多分使わないでしょう
	//float4 StartPos : POSITION1;
	//float4 EndPos : POSITION2;
	float2 UV : TEXCOORD0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};


float2x2 RotationMatrix(float rotation)  
{  
    float c = cos(rotation);  
    float s = sin(rotation);  
 
    return float2x2(c, -s, s ,c);  
}
float4x4 ArbitraryRotateMatrix(float3 axis, float angle)
{
	//public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
    float c = (float)cos(angle);
    float s = (float)sin(angle);
    float t = 1.0f - c;

    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    float4x4 C = mul(c, float4x4(
        1, 0, 0, 0, 
        0, 1, 0, 0, 
        0, 0, 1, 0, 
        0, 0, 0, 0
        ));
    //C.M44 = 1;
	C[3][3] = 1;
    float4x4 T = mul(t, float4x4(
        x * x, x * y, x * z, 0, 
        x * y, y * y, y * z, 0, 
        x * z, y * z, z * z, 0, 
        0, 0, 0, 0
        ));
    float4x4 S = mul(s, float4x4(
        0, z, -y, 0, 
        -z, 0, x, 0, 
        y, -x, 0, 0, 
        0, 0, 0, 0
        ));

	return C + T + S;
}
float3 projectPoint(float3 normal, float3 pos)
{
	//float dis = (pos - StartPos) * normal;
	float dis = dot((pos - StartPos), normal);
	return normal * dis + pos;
}
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float3 position = input.Position;
	// Determine which corner of the rectangle this vertex represents
	float2 offset = float2((input.UV.x - 0.5f) * 2.0f, -(input.UV.y - 0.5f) * 2.0f);

	// Move the vertex along the camera's 'plane' to its corner
	//position = (float3)mul(input.Position, World);

	//float4 tmp = mul(input.Position, World);// input.posが0なので0になっちゃう
	//position = float3(tmp.x, tmp.y, tmp.z);

	float3 pvUp = normalize(cross(ProjectedVector, cross(Up, Side)));
	position += offset.x * Size.x * ProjectedVector + offset.y * Size.y * pvUp;
	//position += offset.x * Size.x * pvUp + offset.y * Size.y * ProjectedVector;
	//position += StartPos + offset.x * Size.x * ProjectedVector + offset.y * Size.y * pvUp;

	/*position += offset.x * Size.x * Side + offset.y * Size.y * Up;// まずは４点構成
	float4 tmp = mul(float4(position, 1), World);
	position = float3(tmp.x, tmp.y, tmp.z);*/
	//position = projectPoint(cross(Up, Side), position);


	// new start and new end
	//float4 ns = normalize(mul(StartPos, World));
	//float4 es = normalize(mul(EndPos, World));
	//position = mul(position, ArbitraryRotateMatrix(cross(Up, Side), theta));
	//position = mul(position, ArbitraryRotateMatrix(CameraDir, theta));


	// Transform the position by view and projection
	output.Position = mul(float4(position, 1), mul(View, Projection));
	output.UV = input.UV;

	return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(texSampler, input.UV);
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