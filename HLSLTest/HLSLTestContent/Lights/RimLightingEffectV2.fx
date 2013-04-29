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
float RimIntensity = 0.3f;         // Intensity of the rim light
 
float3 DiffuseLightDirection;
float3 CameraPosition;
float3 CameraDirection;
float Shinniness = 0.5f;

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


bool AlphaTest = true;
bool AlphaValue = 0.5f;
float CrossTestValue = 0.1f;
bool AlphaTestGreater = true;
float4 CenterToCamera;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord		: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 CameraView : TEXCOORD1;
	float2 TexCoord		: TEXCOORD2;
	//float4 NdotV			: TEXCOORD3;
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

	/*float3 deformedNormal = input.Normal + normalWarp;
	deformedNormal = normalize(mul(deformedNormal, World));
	float3 viewVec = normalize(cameraPos - mul(float4(deformedPos, 1), World));
	Out.NdotV = dot(deformedNormal, viewVec);*/
	output.TexCoord = input.TexCoord;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 lightdir = normalize(DiffuseLightDirection);
	float3 norm = normalize(input.Normal);

	// Calculate the rim lighting component
	// 内積を取ることで周辺部に行くほど0に近い値を得られる。定石らしい
	//float4 rim = pow(1 - dot(norm, CameraDirection), 1.5) * RimColor * RimIntensity;
	//float crossValue = dot(norm, CameraDirection);
	float crossValue = dot(norm, CenterToCamera);
	if (AlphaTest) {
		// discard pixels below a certain threshold:
		//clip((color.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));
		clip((abs(crossValue) - CrossTestValue) * (AlphaTestGreater ? 1 : -1));
	}
	float4 rim = pow(1 - crossValue, 1.5f) * RimColor * RimIntensity;

	// Calculate the specular component
	float3 halfAngle = normalize(lightdir + input.CameraView);
	float4 specular = pow(saturate(dot(norm, halfAngle)), Shinniness) * SpecularColor * SpecularIntensity;
	float4 diffuse = dot(lightdir, input.Normal) * DiffuseColor0 * DiffuseIntensity;
	float4 ambient = AmbientColor * AmbientIntensity;
	//return rim + ambient + specular + diffuse;
	return float4(1, 1, 1, 1);

	/*float4 modulatedColor;
	float4 finalColor;
	float4 baseColor = tex2D(baseMap, input.TexCoord);
	//float4 cubeMapColor = texCUBE(cubeMap, input.ReflectionVec);
	
	modulatedColor.rgb = saturate(2 * baseColor);
	modulatedColor.a = (1-input.Normal)*0.5 - 0.01;		
	//float opacity = saturate(4*(cubeMapColor.a*cubeMapColor.a - 0.75));
	float opacity = 0.5f;

	finalColor.rgb = lerp(modulatedColor, RimColor, opacity);
	finalColor.a = modulatedColor.a + opacity;

	return finalColor;*/
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
