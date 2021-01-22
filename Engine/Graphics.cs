using System;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.Mathematics.Interop;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceChild = SharpDX.Direct3D11.DeviceChild;

using static Engine.Graphics;

namespace Engine
{
    public class ShaderView : Disposable
    {
        public ShaderView(int size)
        {
            resources = new ShaderResourceView[size];
        }

        private readonly ShaderResourceView[] resources;

        internal void Init(Texture2D texture)
        {
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] is not null) continue;

                resources[i] = new ShaderResourceView(device, texture);

                return;
            }
        }

        public void PassToVS()
        {
            context.VertexShader.SetShaderResources(0, resources);
        }
        public void PassToGS()
        {
            context.GeometryShader.SetShaderResources(0, resources);
        }
        public void PassToPS()
        {
            context.PixelShader.SetShaderResources(0, resources);
        }

        public void PassToVS(int resource)
        {
            context.VertexShader.SetShaderResources(0, resources[resource]);
        }
        public void PassToGS(int resource)
        {
            context.GeometryShader.SetShaderResources(0, resources[resource]);
        }
        public void PassToPS(int resource)
        {
            context.PixelShader.SetShaderResources(0, resources[resource]);
        }

        protected override void Dispose(bool disposing)
        {
            resources.Dispose();
        }
    }

    public class RenderView : Disposable
    {
        public static void Reset()
        {
            context.OutputMerger.ResetTargets();
        }

        public RenderView(int width, int height, int count)
        {
            Width = width;
            Height = height;

            var desc = new Texture2DDescription
            {
                Width = Width,
                Height = Height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Format = Format.D24_UNorm_S8_UInt,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None
            };

            using (var buffer = new Texture2D(device, desc))
            {
                stencil = new DepthStencilView(device, buffer);
                views = new RenderTargetView[count];
            }
        }

        private readonly DepthStencilView stencil;
        private readonly RenderTargetView[] views;

        public int Width { get; private set; }
        public int Height { get; private set; }

        internal void Init(Texture2D texture)
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] is not null) continue;

                views[i] = new RenderTargetView(device, texture);

                return;
            }
        }

        public void Fill(ShaderView shaderView)
        {
            var desc = new Texture2DDescription
            {
                Width = Width,
                Height = Height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Format = Format.B8G8R8A8_UNorm,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
            };

            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] is not null) continue;

                using (var tex = new Texture2D(device, desc))
                {
                    Init(tex);
                    shaderView.Init(tex);
                }
            }
        }

        public void SetAsTargetDepthStencilOnly()
        {
            context.OutputMerger.SetTargets(stencil);
            context.Rasterizer.SetViewport(0, 0, Width, Height);
        }
        public void SetAsTarget()
        {
            context.OutputMerger.SetTargets(stencil, views);
            context.Rasterizer.SetViewport(0, 0, Width, Height);
        }
        public void SetAsTarget(int target)
        {
            context.OutputMerger.SetTargets(stencil, views[target]);
            context.Rasterizer.SetViewport(0, 0, Width, Height);
        }

        public void Clear()
        {
            var col = new RawColor4();

            context.ClearDepthStencilView(stencil, DepthStencilClearFlags.Depth, 1, 0);
            context.ClearDepthStencilView(stencil, DepthStencilClearFlags.Stencil, 1, 0);

            foreach (var view in views)
                context.ClearRenderTargetView(view, col);
        }
        public void Clear(Color color)
        {
            var col = new RawColor4(color.R, color.G, color.B, color.A);

            foreach (var view in views)
                context.ClearRenderTargetView(view, col);
        }
        public void Clear(Color color, float depth, byte stencil)
        {
            var col = new RawColor4(color.R, color.G, color.B, color.A);

            context.ClearDepthStencilView(this.stencil, DepthStencilClearFlags.Depth, depth, stencil);
            context.ClearDepthStencilView(this.stencil, DepthStencilClearFlags.Stencil, depth, stencil);

            foreach (var view in views)
                context.ClearRenderTargetView(view, col);
        }
        public void Clear(float depth)
        {
            context.ClearDepthStencilView(stencil, DepthStencilClearFlags.Depth, depth, 0);
        }
        public void Clear(byte stencil)
        {
            context.ClearDepthStencilView(this.stencil, DepthStencilClearFlags.Depth, 1, stencil);
        }
        public void Clear(float depth, byte stencil)
        {
            context.ClearDepthStencilView(this.stencil, DepthStencilClearFlags.Depth, depth, stencil);
            context.ClearDepthStencilView(this.stencil, DepthStencilClearFlags.Stencil, depth, stencil);
        }
        public void Clear(int target)
        {
            var col = new RawColor4();

            context.ClearRenderTargetView(views[target], col);
        }
        public void Clear(int target, Color color)
        {
            var col = new RawColor4(color.R, color.G, color.B, color.A);

            context.ClearRenderTargetView(views[target], col);
        }

        protected override void Dispose(bool disposing)
        {
            stencil.Dispose();
            views.Dispose();
        }
    }

    public class Layout : Disposable
    {
        public Layout(byte[] code)
        {
            using (var reflection = new ShaderReflection(code))
            {
                var count = reflection.Description.InputParameters;
                var elements = new InputElement[count];

                for (int i = 0; i < count; i++)
                {
                    var input = reflection.GetInputParameterDescription(i);
                    var format = Format.Unknown;
                    var name = input.SemanticName;
                    var index = input.SemanticIndex;
                    var slot = name == "INSTANCE" ? 1 : 0;
                    var clas = slot == 1 ? InputClassification.PerInstanceData : InputClassification.PerVertexData;

                    if ((input.UsageMask & RegisterComponentMaskFlags.ComponentW) > 0) format = Format.R32G32B32A32_Float;
                    else if ((input.UsageMask & RegisterComponentMaskFlags.ComponentZ) > 0) format = Format.R32G32B32_Float;
                    else if ((input.UsageMask & RegisterComponentMaskFlags.ComponentY) > 0) format = Format.R32G32_Float;
                    else if ((input.UsageMask & RegisterComponentMaskFlags.ComponentX) > 0) format = Format.R32_Float;

                    elements[i] = new InputElement(name, index, format, -1, slot, clas, slot);
                }

                layout = new InputLayout(device, code, elements);
            }
        }

        private readonly InputLayout layout;

        public void Apply()
        {
            context.InputAssembler.InputLayout = layout;
        }

        protected override void Dispose(bool disposing)
        {
            layout.Dispose();
        }
    }

    public enum ShaderStage
    {
        Vertex,
        Geometry,
        Pixel,
    }

    public class Shader : Disposable
    {
        public static void ClearVS()
        {
            context.VertexShader.Set(null);
        }
        public static void ClearGS()
        {
            context.GeometryShader.Set(null);
        }
        public static void ClearPS()
        {
            context.PixelShader.Set(null);
        }

        public Shader(ShaderStage stage, byte[] code)
        {
            switch (stage)
            {
                case ShaderStage.Vertex:
                    target = context.VertexShader;
                    shader = new VertexShader(device, code);
                    break;
                case ShaderStage.Geometry:
                    target = context.GeometryShader;
                    shader = new GeometryShader(device, code);
                    break;
                case ShaderStage.Pixel:
                    target = context.PixelShader;
                    shader = new PixelShader(device, code);
                    break;
            }

            using (var reflection = new ShaderReflection(code))
            {
                if (reflection.Description.ConstantBuffers is 0) return;

                using (var cbuffer = reflection.GetConstantBuffer(0))
                {
                    var desc = new BufferDescription
                    {
                        SizeInBytes = cbuffer.Description.Size,
                        BindFlags = BindFlags.ConstantBuffer,
                        Usage = ResourceUsage.Default,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None,
                    };

                    buffer = new Buffer(device, desc);
                    data = new byte[desc.SizeInBytes];
                }
            }
        }

        private bool changed;
        private readonly byte[] data;
        private readonly Buffer buffer;
        private readonly CommonShaderStage target;
        private readonly DeviceChild shader;

        public void Apply()
        {
            target.SetShader(shader, null, 0);
        }
        public void Pass()
        {
            if (changed)
            {
                context.UpdateSubresource(data, buffer);
                changed = false;
            }

            target.SetConstantBuffer(0, buffer);
        }

        public void SetValue<T>(T value) where T : struct
        {
            context.UpdateSubresource(ref value, buffer);
        }

        protected override void Dispose(bool disposing)
        {
            data?.Clear();
            buffer?.Dispose();
            shader.Dispose();

        }
    }

    public class Mesh : Disposable
    {
        public static Mesh Create<T>(int[] indices, T[] vertices, Topology topology) where T : struct
        {
            var stride = Utilities.SizeOf<T>();
            var index = Buffer.Create(device, BindFlags.IndexBuffer, indices);
            var vertex = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
            var binding = new VertexBufferBinding(vertex, stride, 0);

            return new Mesh(indices.Length, index, vertex, binding, topology);
        }

        private Mesh(int indexCount, Buffer indices, Buffer vertices, VertexBufferBinding binding, Topology topology)
        {
            this.indexCount = indexCount;
            this.indices = indices;
            this.vertices = vertices;
            this.binding = binding;
            this.topology = (PrimitiveTopology)topology;
        }

        private readonly int indexCount;
        private readonly Buffer indices;
        private readonly Buffer vertices;
        private readonly VertexBufferBinding binding;
        private readonly PrimitiveTopology topology;

        public void Pass()
        {
            context.InputAssembler.PrimitiveTopology = topology;
            context.InputAssembler.SetIndexBuffer(indices, Format.R32_UInt, 0);
            context.InputAssembler.SetVertexBuffers(0, binding);
        }
        public void Draw()
        {
            context.InputAssembler.PrimitiveTopology = topology;
            context.InputAssembler.SetIndexBuffer(indices, Format.R32_UInt, 0);
            context.InputAssembler.SetVertexBuffers(0, binding);
            context.DrawIndexed(indexCount, 0, 0);
        }
        public void DrawGeometry()
        {
            context.DrawIndexed(indexCount, 0, 0);
        }

        protected override void Dispose(bool disposing)
        {
            indices.Dispose();
            vertices.Dispose();
        }
    }

    public class SimpleMesh : Disposable
    {
        public static SimpleMesh Create<T>(T[] vertices, Topology topology) where T : struct
        {
            var stride = Utilities.SizeOf<T>();
            var vertex = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
            var binding = new VertexBufferBinding(vertex, stride, 0);

            return new SimpleMesh(vertices.Length, vertex, binding, topology);
        }

        private SimpleMesh(int vertexCount, Buffer vertices, VertexBufferBinding binding, Topology topology)
        {
            this.vertexCount = vertexCount;
            this.vertices = vertices;
            this.binding = binding;
            this.topology = (PrimitiveTopology)topology;
        }

        private readonly int vertexCount;
        private readonly Buffer vertices;
        private readonly VertexBufferBinding binding;
        private readonly PrimitiveTopology topology;

        public void Pass()
        {
            context.InputAssembler.PrimitiveTopology = topology;
            context.InputAssembler.SetVertexBuffers(0, binding);
        }
        public void Draw()
        {
            context.InputAssembler.PrimitiveTopology = topology;
            context.InputAssembler.SetVertexBuffers(0, binding);
            context.Draw(vertexCount, 0);
        }
        public void DrawGeometry()
        {
            context.Draw(vertexCount, 0);
        }

        protected override void Dispose(bool disposing)
        {
            vertices.Dispose();
        }
    }

    public struct Vertex
    {
        public Vertex(float px, float py, float pz, float nx, float ny, float nz, float u, float v)
        {
            this.px = px;
            this.py = py;
            this.pz = pz;
            this.nx = nx;
            this.ny = ny;
            this.nz = nz;
            this.u = u;
            this.v = v;
        }
        public Vertex(Vector p, Vector n, Vector t) : this(p.X, p.Y, p.Z, n.X, n.Y, n.Z, t.X, t.Y)
        {

        }

        private readonly float px, py, pz;
        private readonly float nx, ny, nz;
        private readonly float u, v;
    }

    public struct Vertex1
    {
        public Vertex1(float px, float py, float pz, float pw, float u, float v)
        {
            this.px = px;
            this.py = py;
            this.pz = pz;
            this.pw = pw;
            this.u = u;
            this.v = v;
        }

        private readonly float px, py, pz, pw;
        private readonly float u, v;
    }

    public enum Topology : byte
    {
        List = 4,
        Strip = 5,

        ListAdj = 12,
        StripAdj = 13,

        PatchListWith3ControlPoints = 35,
        PatchListWith4ControlPoints = 36,
        PatchListWith6ControlPoints = 38,
    }
}
