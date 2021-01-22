Texture2D tex : register(t0);
sampler _sampler : register(s0);

struct VertexIn
{
    float2 pos : POSITION0;
    float2 tex : TEXCOORD0;
};

struct Vertex
{
    float4 pos : SV_Position;
    float2 tex : TEXCOORD0;
};

void vs(in VertexIn input, out Vertex output)
{
    output.pos = float4(input.pos, 0, 1);
    output.tex = input.tex;
}

void combine(in Vertex input, out float4 color : SV_Target)
{
    color = tex.Sample(_sampler, input.tex);
}

void fxaa(in Vertex input, out float4 color : SV_Target)
{
    color = tex.Sample(_sampler, input.tex);
}