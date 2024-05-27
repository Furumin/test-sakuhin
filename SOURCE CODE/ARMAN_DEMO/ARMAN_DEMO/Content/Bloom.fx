#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
matrix World;
float contrast;
bool horizontal;
sampler s0;
sampler2D Sampler : register(S0);

float weightMul, iterationCount;


#define PI 3.14f

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    //float4 Normal : NORMAL0;
    float2 Texture : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 Color0 : COLOR0;
    float2 Texture : TEXCOORD0;
};

float4 ChangeSaturation(float2 Tex : TEXCOORD0) : COLOR0
{
    return tex2D(Sampler, Tex) * contrast;
}

float4 TestBlurUNI(bool hori, float2 input : TEXCOORD0) : COLOR0
{
    float2 texOffset = float2(1.0 / 1366.0, 1.0 / 768.0);
    float offsetX[] = { 0.0, 1 * texOffset.x, 2 * texOffset.x, 3 * texOffset.x, 4 * texOffset.x, 5 * texOffset.x};
    float offsetY[] = { 0.0, 1 * texOffset.y, 2 * texOffset.y, 3 * texOffset.y, 4 * texOffset.y, 5 * texOffset.y};
    float weight[] = { 0.3134545 * weightMul, 0.2270270270 * weightMul, weightMul*0.193359375, weightMul * 0.12084960937, weightMul*0.0537109375, weightMul*0.01611328125, weightMul*0.0048828125};
    float4 color = tex2D(Sampler, input) * weight[0];
    
    //offset[]の範囲外になる時のエラー予防は行っていない
    for (uint i = 1; i < iterationCount; i++)
    {
        float2 of;
        if (hori)
            of = float2(offsetX[i], 0);
        else
        {
            of = float2(0, offsetY[i]);
        }
        color += tex2D(Sampler, float2(input.xy + of)) * weight[i];
        color += tex2D(Sampler, float2(input.xy - of)) * weight[i];
    }

    return color;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color0 = input.Color;
    output.Texture = input.Texture;

    return output;
}

float4 MainPS1_1(VertexShaderOutput input) : COLOR0
{
    float4 color = TestBlurUNI(horizontal, input.Texture);
    return color;
}

technique Bloom
{
    //pass P0
    //{
    //    //VertexShader = compile VS_SHADERMODEL MainVS();
    //    PixelShader = compile PS_SHADERMODEL MainPS0();
    //}
    pass P1
    {
        //VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS1_1();
    }
};