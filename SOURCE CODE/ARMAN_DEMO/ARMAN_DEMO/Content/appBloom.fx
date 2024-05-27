#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

Texture2D Texture0;
sampler2D Sampler0 = sampler_state
{
    Texture = <Texture0>;
};

Texture2D Texture1;
sampler2D Sampler1 = sampler_state
{
    Texture = <Texture1>;
};
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 Texture0 : TEXCOORD0;
    float2 Texture1 : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = input.Color;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float gamma = 2.2;
    float3 clr = tex2D(Sampler0, input.Texture0).rgb;
    float3 bloom = tex2D(Sampler1, input.Texture0).rgb;
    clr += bloom;

	clr.x = 1.0 - exp(-clr.x * 3.0);
	clr.y = 1.0 - exp(-clr.y * 3.0);
	clr.z = 1.0 - exp(-clr.z * 3.0);

    clr.x = pow(clr.x, float(1.0 / gamma));
    clr.y = pow(clr.y, float(1.0 / gamma));
    clr.z = pow(clr.z, float(1.0 / gamma));
    return float4(clr, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		//VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};