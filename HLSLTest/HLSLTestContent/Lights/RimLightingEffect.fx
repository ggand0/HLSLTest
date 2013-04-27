float4x4 World;
float4x4 View;
float4x4 Projection;

float4 AmbientColor = float4(.2, .2, .2, 0.1f);
float4 DiffuseColor0 = float4(.2, .2, .2, 0.1f);//float4(1, 1, 1, 0.1f);// 例外対策で名前を変えている
float4 SpecularColor = float4(.2, .2, .2, 0.1f);//float4(1, 1, 1, 0.1f);
float4 RimColor;
 
float AmbientIntensity = 0.5f;
float DiffuseIntensity = 0.5f;
float SpecularIntensity = 0.5f;
float RimIntensity = 0.5f;         // Intensity of the rim light
 
float3 DiffuseLightDirection;
float3 CameraPosition;
float3 CameraDirection;
float Shinniness = 0.5f;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 CameraView : TEXCOORD1;
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

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 lightdir = normalize(DiffuseLightDirection);
	float3 norm = normalize(input.Normal);

	// Calculate the rim lighting component
	// 内積を取ることで周辺部に行くほど0に近い値を得られる。定石らしい
	float4 rim = pow(1 - dot(norm, CameraDirection), 1.5) * RimColor * RimIntensity;

	// Calculate the specular component
	float3 halfAngle = normalize(lightdir + input.CameraView);
	float4 specular = pow(saturate(dot(norm, halfAngle)), Shinniness) * SpecularColor * SpecularIntensity;

	float4 diffuse = dot(lightdir, input.Normal) * DiffuseColor0 * DiffuseIntensity;
	float4 ambient = AmbientColor * AmbientIntensity;

    return rim + ambient + specular + diffuse;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
