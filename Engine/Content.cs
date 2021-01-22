using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

using System;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Drawing.Imaging;
using System.Collections.Generic;

using Point1 = System.Drawing.Point;
using PixelFormat1 = System.Drawing.Imaging.PixelFormat;

using static Engine.Graphics;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct3D;

namespace Engine
{
    public static class HLSL
    {
        static HLSL()
        {
            var desc1 = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Front,

                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,

                DepthBias = 0,
                DepthBiasClamp = 0,
                SlopeScaledDepthBias = 0
            };
            var desc2 = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,

                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,

                DepthBias = 0,
                DepthBiasClamp = 0,
                SlopeScaledDepthBias = 0
            };

            var desc3 = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,

                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Maximum,

                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Maximum,

                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };
            var desc4 = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,

                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,

                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Maximum,

                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };

            var desc5 = new BlendStateDescription
            {
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = true,
            };
            var desc6 = new BlendStateDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = true,
            };

            var desc7 = new DepthStencilStateDescription
            {
                IsDepthEnabled = false,
                IsStencilEnabled = false,
            };
            var desc8 = new DepthStencilStateDescription
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,

                IsStencilEnabled = false,
            };
            var desc9 = new DepthStencilStateDescription
            {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,

                IsStencilEnabled = true,
                StencilReadMask = 0xff,
                StencilWriteMask = 0x00,
                
                FrontFace = new DepthStencilOperationDescription
                {
                    Comparison = Comparison.Equal,
                    PassOperation = StencilOperation.Keep,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                },
                BackFace = new DepthStencilOperationDescription
                {
                    Comparison = Comparison.Equal,
                    PassOperation = StencilOperation.Keep,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                }
            }; 
            var desc10 = new DepthStencilStateDescription
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Less,

                IsStencilEnabled = true,
                StencilReadMask = 0x00,
                StencilWriteMask = 0xff,

                FrontFace = new DepthStencilOperationDescription
                {
                    Comparison = Comparison.Always,
                    PassOperation = StencilOperation.Increment,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                },
                BackFace = new DepthStencilOperationDescription
                {
                    Comparison = Comparison.Always,
                    PassOperation = StencilOperation.Decrement,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                }
            };

            for (int i = 0; i < 8; i++)
            {
                desc5.RenderTarget[i] = desc3;
                desc6.RenderTarget[i] = desc4;
            }

            DefaultRasterizer = new RasterizerState(device, desc1);
            DoubleSide = new RasterizerState(device, desc2);

            DefaultBlend = new BlendState(device, desc5);
            AdditiveBlend = new BlendState(device, desc6);

            NoDepth = new DepthStencilState(device, desc7);
            DefaultDepth = new DepthStencilState(device, desc8);
            EqualStencil = new DepthStencilState(device, desc9);
            IncDecStencil = new DepthStencilState(device, desc10);

            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                DefaultRasterizer.Dispose();
                DoubleSide.Dispose();
            };
        }

        public static readonly RasterizerState DefaultRasterizer;
        public static readonly RasterizerState DoubleSide;

        public static readonly BlendState DefaultBlend;
        public static readonly BlendState AdditiveBlend;

        public static readonly DepthStencilState NoDepth;
        public static readonly DepthStencilState DefaultDepth;
        public static readonly DepthStencilState EqualStencil;
        public static readonly DepthStencilState IncDecStencil;

        public static void Apply(this DepthStencilState blend)
        {
            context.OutputMerger.SetDepthStencilState(blend);
        }
        public static void Apply(this RasterizerState blend)
        {
            context.Rasterizer.State = blend;
        }
        public static void Apply(this BlendState blend)
        {
            context.OutputMerger.SetBlendState(blend);
        }

        public static Shader CompileShader(string path, ShaderStage stage)
        {
            if (!path.EndsWith(".hlsl")) path += ".hlsl";

            var entry = stage switch
            {
                ShaderStage.Vertex => "MainVS",
                ShaderStage.Geometry => "MainGS",
                ShaderStage.Pixel => "MainPS",
            };

            var profile = stage switch
            {
                ShaderStage.Vertex => "vs_5_0",
                ShaderStage.Geometry => "gs_5_0",
                ShaderStage.Pixel => "ps_5_0",
            };

            try
            {
                using (var code = ShaderBytecode.CompileFromFile(path, entry, profile))
                    return new Shader(stage, code);
            }
            catch (Exception e)
            {
                Debug.Throw($"Error while loading Technique \"{path}\"\n\nMessage:\n\n" + e.Message);

                return null;
            }
        }
        public static Layout CompileLayout(string path, ShaderStage stage)
        {
            if (!path.EndsWith(".hlsl")) path += ".hlsl";

            var entry = stage switch
            {
                ShaderStage.Vertex => "MainVS",
                ShaderStage.Geometry => "MainGS",
                ShaderStage.Pixel => "MainPS",
            };

            var profile = stage switch
            {
                ShaderStage.Vertex => "vs_5_0",
                ShaderStage.Geometry => "gs_5_0",
                ShaderStage.Pixel => "ps_5_0",
            };

            using (var code = TryGetCode(path, entry, profile))
                return new Layout(code);
        }

        private static ShaderBytecode TryGetCode(string path, string entry, string profile)
        {
            try
            {
                return ShaderBytecode.CompileFromFile(path, entry, profile);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("entrypoint not found"))
                {
                    string message = $"Error while loading Technique \"{path}\"\n\nMessage:\n\n";

                    Debug.Throw(message + e.Message);
                }
                return null;
            }
        }
    }

    public static class PNG
    {
        public static void FromFile(ref ShaderView buffer, string path)
        {
            if (!path.EndsWith(".png")) path += ".png";

            using (var texture = CreateTexture(path))
                buffer.Init(texture);
        }

        private static Texture2D CreateTexture(string path)
        {
            using (var image = Image.FromFile(path))
            using (var bitmap = new Bitmap(image))
            {
                var rectangle = new Rectangle(Point1.Empty, bitmap.Size);
                var data = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat1.Format32bppArgb);

                var desc = new Texture2DDescription()
                {
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    ArraySize = 1,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Format = Format.B8G8R8A8_UNorm,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                };
                var bytes = new DataRectangle(data.Scan0, data.Stride);

                var ret = new Texture2D(device, desc, bytes);
                bitmap.UnlockBits(data);
                return ret;
            }
        }
    }

    public static class OBJ
    {
        public static Mesh FromFile(string path)
        {
            if (!path.EndsWith(".obj")) path += ".obj";

            List<Vector> pos = new();
            List<Vector> nor = new();
            List<Vector> tex = new();

            List<Vertex> vertices = new();
            List<int> indices = new();

            try
            {
                using var stream = File.OpenRead(path);
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split(' ');

                    switch (line[0])
                    {
                        case "v":
                            ReadVector3(pos, line);
                            break;
                        case "vn":
                            ReadVector3(nor, line);
                            break;
                        case "vt":
                            ReadVector2(tex, line);
                            break;
                        case "f":
                            ReadTriangle(vertices, indices, pos, nor, tex, line);
                            break;
                    }
                }

                return Mesh.Create(indices.ToArray(), vertices.ToArray(), Topology.List);
            }
            catch (Exception e)
            {
                var message = $"Error while loading Mesh \"{path}\"\n\nMessage:\n\n";

                Debug.Throw(message + e.Message);

                return null;
            }
        }

        private static void ReadVector3(List<Vector> vectors, string[] line)
        {
            var x = float.Parse(line[1], CultureInfo.InvariantCulture);
            var y = float.Parse(line[2], CultureInfo.InvariantCulture);
            var z = float.Parse(line[3], CultureInfo.InvariantCulture);

            vectors.Add(new Vector(x, y, z));
        }
        private static void ReadVector2(List<Vector> vectors, string[] line)
        {
            var u = float.Parse(line[1], CultureInfo.InvariantCulture);
            var v = float.Parse(line[2], CultureInfo.InvariantCulture);

            vectors.Add(new Vector(u, v, 0));
        }
        private static void ReadTriangle(List<Vertex> vertices, List<int> indices, List<Vector> pos, List<Vector> nor, List<Vector> tex, string[] line)
        {
            var p0 = ReadVertex(pos, nor, tex, line[1].Split('/'));
            var p1 = ReadVertex(pos, nor, tex, line[2].Split('/'));
            var p2 = ReadVertex(pos, nor, tex, line[3].Split('/'));

            var i0 = vertices.IndexOf(p0);
            var i1 = vertices.IndexOf(p1);
            var i2 = vertices.IndexOf(p2);

            if (i0 < 0)
            {
                indices.Add(vertices.Count);
                vertices.Add(p0);
            }
            else
            {
                indices.Add(i0);
            }

            if (i1 < 0)
            {
                indices.Add(vertices.Count);
                vertices.Add(p1);
            }
            else
            {
                indices.Add(i1);
            }

            if (i2 < 0)
            {
                indices.Add(vertices.Count);
                vertices.Add(p2);
            }
            else
            {
                indices.Add(i2);
            }
        }

        private static Vertex ReadVertex(List<Vector> pos, List<Vector> nor, List<Vector> tex, string[] xyz)
        {
            var p = int.Parse(xyz[0], CultureInfo.InvariantCulture) - 1;
            var t = int.Parse(xyz[1], CultureInfo.InvariantCulture) - 1;
            var n = int.Parse(xyz[2], CultureInfo.InvariantCulture) - 1;

            return new Vertex
            (
                pos[p].X, pos[p].Y, pos[p].Z,
                nor[n].X, nor[n].Y, nor[n].Z,
                tex[t].X, tex[t].Y
            );
        }
    }

    public static class MDL
    {
        public static Mesh FromFile(string path)
        {
            if (!path.EndsWith(".mdl")) path += ".mdl";

            Vertex[] vertices;
            int[] indices;

            using (FileStream file = File.OpenRead(path))
            using (BinaryReader reader = new BinaryReader(file))
            {
                int vertexCount = reader.ReadInt32();
                int indiciesCount = reader.ReadInt32();

                vertices = new Vertex[vertexCount];
                indices = new int[indiciesCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i] = new Vertex
                    (
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                    );
                }

                for (int i = 0; i < indiciesCount; i++)
                {
                    indices[i] = reader.ReadInt32();
                }
            }

            return Mesh.Create(indices, vertices, Topology.ListAdj);
        }
        public static Mesh FromFile1(string path)
        {
            if (!path.EndsWith(".mdl")) path += ".mdl";

            Vertex[] vertices;
            int[] indices;

            using (FileStream file = File.OpenRead(path))
            using (BinaryReader reader = new BinaryReader(file))
            {
                int vertexCount = reader.ReadInt32();
                int indiciesCount = reader.ReadInt32()/2;

                vertices = new Vertex[vertexCount];
                indices = new int[indiciesCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i] = new Vertex
                    (
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                    );
                }

                for (int i = 0; i < indiciesCount; i++)
                {
                    indices[i] = reader.ReadInt32();
                    reader.ReadInt32();
                }
            }

            return Mesh.Create(indices, vertices, Topology.List);
        }
    }
}
