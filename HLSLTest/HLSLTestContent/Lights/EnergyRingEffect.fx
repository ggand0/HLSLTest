// rim lighting + texture + edge refraction

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 AmbientColor = 0;//float4(.2, .2, .2, 0.01f);
float4 DiffuseColor0 = 0;//float4(.2, .2, .2, 0.01f);//float4(1, 1, 1, 0.1f);// 例外対策で名前を変えている
float4 SpecularColor = 0;//float4(.2, .2, .2, 0.01f);//float4(1, 1, 1, 0.1f);
float4 RimColor;
 
float AmbientIntensity = 0.5f;
float DiffuseIntensity = 0.5f;
float SpecularIntensity = 0.5f;
float RimIntensity = 1.5;//0.3f;         // Intensity of the rim light
 
float3 DiffuseLightDirection;
float3 CameraPosition;
float3 CameraDirection;
float Shinniness = 0.8f;

texture BaseTexture;
sampler2D baseMap = sampler_state
{
	Texture = <BaseTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

float4x4 matInverseWorld;
float4 vecEye;
texture RefractionMap;
samplerCUBE RefractionSampler = sampler_state {
	texture = <RefractionMap>;
};
float3 RefractColor;


bool AlphaTest = true;
bool AlphaValue = 0.5f;
float CrossTestValue = 0.1f;
bool AlphaTestGreater = true;
float4 CenterToCamera;


texture NormalMap;
	sampler2D NormalSampler = sampler_state {
	texture = <NormalMap>;
};
float WaveLength = 0.6;
float WaveHeight = 0.2;
float Time = 0;
float WaveSpeed = 0.5f;//0.04f;


struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord		: TEXCOORD0;
	//float2 UV : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 CameraView : TEXCOORD1;
	float2 TexCoord		: TEXCOORD2;
	float3 ViewDirection : TEXCOORD3;
	float4 PositionCopy : TEXCOORD4;
	float2 NormalMapPosition : TEXCOORD5;
	//float2 UV : TEXCOORD5;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.Normal = mul(input.Normal, World);

	// get the vector from the camera to the vertex
	output.CameraView = normalize(CameraPosition - worldPosition);

    /*float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);*/
	
	output.PositionCopy = output.Position;
	//output.Normal = normalize(mul(matInverseWorld, input.Normal));
	//output.ViewDirection = normalize(vecEye - output.Position);
	output.TexCoord = input.TexCoord;

	float3 PosWorldr = (mul(input.Position, World));
	float3 ViewDir = normalize(PosWorldr - vecEye);
	//float3 ViewDir = normalize(vecEye - PosWorldr);
	output.ViewDirection = ViewDir;
	//output.UV = input.UV;

	// animation
	output.NormalMapPosition = input.TexCoord / WaveLength;
	output.NormalMapPosition.y -= Time * WaveSpeed;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 lightdir = normalize(DiffuseLightDirection);
	float3 norm = normalize(input.Normal);

	// Calculate the rim lighting component
	// 内積を取ることで周辺部に行くほど0に近い値を得られる。
	//float4 rim = pow(1 - dot(norm, CameraDirection), 1.5) * RimColor * RimIntensity;
	//float crossValue = dot(norm, CameraDirection);
	float crossValue = dot(norm, CenterToCamera);
	if (AlphaTest) {
		clip((abs(crossValue) - CrossTestValue) * (AlphaTestGreater ? 1 : -1));
	}
	float4 rim = pow(1 - crossValue, 1.5f) * RimColor * RimIntensity;

	// calc refraction + animation
	float4 normal = tex2D(NormalSampler, input.NormalMapPosition) * 2 - 1;
	float2 UVOffset = WaveHeight * normal.rg;
	float3 Refract = refract(input.ViewDirection, input.Normal, 0.5);
	float3 RefractColor0 = texCUBE(RefractionSampler, Refract);// texCUBEだから座標もfloat3なわけか

	// base texture
	//rim *= tex2D(baseMap, input.TexCoord + UVOffset * 10);

	// Calculate the specular component
	float3 halfAngle = normalize(lightdir + input.CameraView);
	float4 specular = pow(saturate(dot(norm, halfAngle)), Shinniness) * SpecularColor * SpecularIntensity;
	float4 diffuse = dot(lightdir, input.Normal) * DiffuseColor0 * DiffuseIntensity;
	float4 ambient = AmbientColor * AmbientIntensity;


	//return (rim + ambient + specular + diffuse)	+ float4(RefractColor0, 0.5f);
	//return rim + float4(RefractColor0, 0.1f) * 0.05f  + ambient + specular + diffuse;
	return rim;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
