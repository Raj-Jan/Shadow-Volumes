Texture2D _texture;
sampler _sampler;

cbuffer vs
{
	float2 position;
	float2 aspect;
	float size;
};

cbuffer ps
{
	float4 col;
};

struct VertexIn
{
	float2 pos : POSITION0;
	float2 tex : TEXCOORD0;

	float2 off : INSTANCE0;
};
struct VertexOut
{
	float4 pos : SV_Position0;
	float2 tex : TEXCOORD0;
};

void MainVS(in VertexIn input, uint id : SV_InstanceId, out VertexOut output)
{
	output.pos.xy = aspect * input.pos + float2(size * (id + input.off.y), 0) + position;
	output.pos.z = 0;
	output.pos.w = 1;

	output.tex = input.tex + float2(input.off.x, 0);
}

void MainPS(in VertexOut input, out float4 color : SV_Target)
{
	color = col * _texture.Sample(_sampler, input.tex);
}

DepthStencilState stencil
{
	DepthEnable = false;
	DepthWriteMask = Zero;
	DepthFunc = Always;

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
};