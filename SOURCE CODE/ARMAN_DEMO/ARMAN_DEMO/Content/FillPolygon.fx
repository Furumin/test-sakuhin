#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

float4 fillColor;
struct Boundaries
{
    float2 b1;
    float2 b2;
    float2 b3;
};

float cross2D(float2 a, float2 b)
{
    return (a.x * b.y) - (a.y * b.x);
}

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
    //output.Position = float4(input.Position.x, input.Position.y, 0, 1);
    output.Color = float4(1.0f, 0.0f, 0.0f, 1.0f);
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 output = input.Color;
    return output;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};