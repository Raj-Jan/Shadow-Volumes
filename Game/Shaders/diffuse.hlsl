
Texture2D _texture;
sampler _sampler;

cbuffer vs
{
    float4x4 transform;
    float3 lightDir;
};

cbuffer ps
{
    float4 ambient;
    float4 diffuse;
};

struct VertexIn
{
    float3 pos : POSITION0;
    float3 nor : NORMAL0;
    float2 tex : TEXCOORD0;
};
struct VertexOut
{
    float4 pos : SV_Position;
    float2 tex : TEXCOORD0;
    float light : TEXCOORD1;
};
struct Pixel
{
    float4 col1 : SV_Target0;
    float4 col2 : SV_Target1;
};

void MainVS(in VertexIn input, out VertexOut output)
{
    output.pos = mul(transform, float4(input.pos, 1));
    output.tex = input.tex;
    output.light = saturate(dot(lightDir, input.nor));
}

void MainPS(in VertexOut input, out Pixel pixel)
{
    float4 tex = _texture.Sample(_sampler, input.tex);
    
    pixel.col1 = ambient * tex;
    pixel.col2 = diffuse * input.light * tex;
}

DepthStencilState stencil
{
    DepthEnable = true;
    DepthWriteMask = All;
    DepthFunc = Less;

    StencilEnable = false;
    StencilReadMask = 0x00;
    StencilWriteMask = 0x00;

    FrontFace_StencilFunc = Always;
    FrontFace_StencilPassOp = Keep;
    FrontFace_StencilFailOp = Keep;
    FrontFace_StencilDepthFailOp = Keep;

    BackFace_StencilFunc = Always;
    BackFace_StencilPassOp = Keep;
    BackFace_StencilFailOp = Keep;
    BackFace_StencilDepthFailOp = Keep;
};

RasterizerState rasterizer
{
    FillMode = Solid;
    CullMode = Front;

    FrontCounterClockwise = false;
    DepthClipEnable = true;
    ScissorEnable = false;
    MultisampleEnable = true;
    AntialiasedLineEnable = true;

    DepthBias = 0;
    DepthBiasClamp = 0;
    SlopeScaledDepthBias = 0;
};

BlendState blend
{
    IndependentBlendEnable = false;
    AlphaToCoverageEnable = true;

    BlendEnable[0] = false;

    SrcBlend[0] = One;
    DestBlend[0] = Zero;
    BlendOp[0] = Maximum;

    SrcBlendAlpha[0] = Zero;
    DestBlendAlpha[0] = One;
    BlendOpAlpha[0] = Maximum;

    RenderTargetWriteMask[0] = All;

    BlendEnable[1] = false;

    SrcBlend[1] = One;
    DestBlend[1] = Zero;
    BlendOp[1] = Maximum;

    SrcBlendAlpha[1] = Zero;
    DestBlendAlpha[1] = One;
    BlendOpAlpha[1] = Maximum;

    RenderTargetWriteMask[1] = All;
};