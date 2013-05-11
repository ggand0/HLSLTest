float4x4 Wordld;
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

bool FaceCamera = true;
bool LineBillboard = false;
float3 CameraPosition;
float3 ProjectedVector;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Direction : TEXCOORD1;
	float Rotation : COLOR0;
	float Speed : TEXCOORD2;
	float StartTime : TEXCOORD3;
	float4 DirectedPosition : POSITION1;
	//float4 DirectedPosition : TEXCOORD4;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	//float4 WorldPosition : TEXCOORD2;
	float2 UV : TEXCOORD0;
	float2 RelativeTime : TEXCOORD1;
	float Rotation : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float3 position = input.Position;


	

	// Move to billboard corner
	float2 offset = Size * float2((input.UV.x - 0.5f) * 2.0f, -(input.UV.y - 0.5f) * 2.0f);

	if (LineBillboard) {
		// nice tryÇ≈ÇÕÇ†Ç¡ÇΩÇ™Ç±ÇÃï˚ñ@Ç≈ÇÕñ≥óùÅBéÀâeÇ≥ÇπÇÈÇµÇ©Ç»Ç¢
		/*float3 NextPos = position + normalize(input.Direction) * Size.y;
		float3 ToCamera = normalize(CameraPosition - position);
		float3 ToDir = normalize(NextPos - position);
		float3 side = normalize(cross(ToCamera ,ToDir));
		position += offset.x * side + offset.y * ToDir;*/

		// Move the vertex along the camera's 'plane' to its corner
		//float3 NextPos = position + normalize(input.Direction) * Size.y;
		//float3 ToDir = normalize(NextPos - position);

		float4 inpos = input.Position;
		float4 indir = input.DirectedPosition;

		float3 startPos = input.Position.xyz / input.Position.w;
		float3 endPos = input.DirectedPosition.xyz / input.DirectedPosition.w;
		float3 lineDir = normalize( endPos - startPos );
		//float3 pvUp = normalize(cross(ProjectedVector, cross(Up, Side)));
		float3 pvUp = normalize(cross(lineDir , cross(Up, Side)));

		//position += offset.x * Size.x * ProjectedVector + offset.y * Size.y * pvUp;
		//position += offset.x * Size.x * float3(lineDir2D, 0) + offset.y * Size.y * pvUp;
		position += offset.x * lineDir + offset.y * pvUp;
	} else {
		position += offset.x * Side + offset.y * Up;
	}

	// Determine how long this particle has been alive
	float relativeTime = (Time - input.StartTime);
	output.RelativeTime = relativeTime;

	// Move the vertex along its movement direction and the wind direction
	position += (input.Direction * input.Speed + Wind) * relativeTime;

	// Transform the final position by the view and projection matrices
	/*if (FaceCamera) {
		output.Position = mul(float4(position, 1), mul(View, Projection));
	} else {
		output.Position = mul(input.Position, mul(View, Projection));
	}*/
	output.Position = mul(float4(position, 1), mul(View, Projection));
	output.UV = input.UV;
	output.Rotation = (input.Rotation + 3.14159) / 6.283185;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Ignore particles that aren't active
	clip(input.RelativeTime);
	
	/*if (!FaceCamera) {
		float4 color = tex2D(texSampler, input.UV);
		return color;
	}*/
	float r = (input.Rotation * 6.283185) - 3.141593;
	float c = cos(r);
	float s = sin(r);
	float2x2 rotationMatrix = float2x2(c, -s, s, c);

	float2 texCoord = mul(input.UV - 0.5, rotationMatrix);
	//return tex2D(TextureSampler, texCoord + 0.5) * i.Colour;

	// Sample texture
	//float4 color = tex2D(texSampler, input.UV);
	float4 color = tex2D(texSampler, texCoord + 0.5);


	// Fade out towards end of life
	float d = clamp(1.0f - pow((input.RelativeTime / Lifespan), 10), 0, 1);
	// Fade in at beginning of life
	d *= clamp((input.RelativeTime / FadeInTime), 0, 1);

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
