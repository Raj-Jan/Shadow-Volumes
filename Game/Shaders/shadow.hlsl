// =============================== Constants ===============================

static float bias = 0.001;

// =============================== Structures ===============================

struct VertexIn
{
    float3 pos : POSITION0;
    float3 nor : NORMAL0;
    float2 tex : TEXCOORD0;
};
struct VertexOut
{
    float4 pos : SV_Position;
};

// =============================== Buffers ===============================

cbuffer vs
{
    float4x4 transform;
};
cbuffer gs
{
    VertexOut extend;
};

// =============================== Shaders ===============================

void MainVS(in VertexIn input, out VertexOut output)
{
    output.pos = mul(transform, float4(input.pos, 1));
}

[maxvertexcount(6)]
void MainGS(in triangleadj VertexOut input[6], inout TriangleStream<VertexOut> stream)
{
    float3 n = cross(input[2].pos.xyz - input[0].pos.xyz, input[4].pos.xyz - input[0].pos.xyz);
    float d = dot(n, extend.pos.xyz);

    if (d < bias)
        return;

    bool b = false;
    
    n = cross(input[1].pos.xyz - input[0].pos.xyz, input[2].pos.xyz - input[0].pos.xyz);
    d = dot(n, extend.pos.xyz);

    if (d < bias)
    {
        stream.Append(input[0]);
        stream.Append(input[2]);
        stream.Append(extend);
        
        b = true;
    }

    n = cross(input[2].pos.xyz - input[4].pos.xyz, input[3].pos.xyz - input[4].pos.xyz);
    d = dot(n, extend.pos.xyz);

    if (d < bias)
    {
        if (b)
        {
            stream.Append(input[4]);
        }
        else
        {
            stream.Append(input[2]);
            stream.Append(input[4]);
            stream.Append(extend);
            
            b = true;
        }
    }
    else if (b)
    {
        stream.RestartStrip();
        b = false;
    }

    n = cross(input[4].pos.xyz - input[0].pos.xyz, input[5].pos.xyz - input[0].pos.xyz);
    d = dot(n, extend.pos.xyz);

    if (d < bias)
    {   
        if (b)
        {
            stream.Append(input[0]);
        }
        else
        {
            stream.Append(input[4]);
            stream.Append(input[0]);
            stream.Append(extend);
        }
        
        stream.RestartStrip();
    }
}

// =============================== States ===============================

DepthStencilState stencil
{
    DepthEnable = true;
    DepthWriteMask = Zero;
    DepthFunc = Less;

    StencilEnable = true;
    StencilReadMask = 0x00;
    StencilWriteMask = 0xFF;

    FrontFace_StencilFunc = Always;
    FrontFace_StencilPassOp = Increment;
    FrontFace_StencilFailOp = Keep;
    FrontFace_StencilDepthFailOp = Keep;

    BackFace_StencilFunc = Always;
    BackFace_StencilPassOp = Decrement;
    BackFace_StencilFailOp = Keep;
    BackFace_StencilDepthFailOp = Keep;
};

RasterizerState rasterizer
{
    FillMode = Solid;
    CullMode = None;

    FrontCounterClockwise = false;
    DepthClipEnable = true;
    ScissorEnable = false;
    MultisampleEnable = true;
    AntialiasedLineEnable = true;

    DepthBias = 0;
    DepthBiasClamp = 0;
    SlopeScaledDepthBias = 0;
};