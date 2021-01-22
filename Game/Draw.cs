using Engine;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;

using static Engine.Graphics;

using Resource = Engine.Resource;

namespace Game
{
    public class DirectionalLight
    {
        public Color Ambient { get; set; }
        public Color Diffuse { get; set; }
        public Vector Direction { get; set; }

        public static void Begin()
        {
            Shaders.layout.Apply();
        }

        public void DrawShadow(ICamera camera, IEnumerable<IRender> entities)
        {
            HLSL.DoubleSide.Apply();
            HLSL.IncDecStencil.Apply();

            Shaders.shadow_VS.Apply();
            Shaders.shadow_GS.Apply();

            var dir = camera.View * new Color(Direction, 0);

            Shaders.shadow_GS.SetValue(dir);
            Shaders.shadow_GS.Pass();

            foreach (var entity in entities)
            {
                Shaders.shadow_VS.SetValue(camera.View * entity.World);
                Shaders.shadow_VS.Pass();

                entity.Shape();
            }
        }
        public void DrawDiffuse(ICamera camera, IEnumerable<IRender> entities)
        {
            HLSL.DefaultDepth.Apply();
            HLSL.DefaultRasterizer.Apply();
            HLSL.DefaultBlend.Apply();

            Shaders.diffuse_VS.Apply();
            Shaders.diffuse_PS.Apply();

            var data1 = new Data1()
            {
                ambient = Ambient,
                diffuse = Diffuse,
            };

            Shaders.diffuse_PS.SetValue(data1);
            Shaders.diffuse_PS.Pass();

            foreach (var entity in entities)
            {
                var data = new Data()
                {
                    matrix = camera.View * entity.World,
                    dir = ~entity.World * new Color(-Direction, 0)
                };

                Shaders.diffuse_VS.SetValue(data);
                Shaders.diffuse_VS.Pass();

                entity.Draw();
            }
        }

        private struct Data
        {
            public Matrix matrix;
            public Color dir;
        }

        private struct Data1
        {
            public Color ambient;
            public Color diffuse;
        }

        private class Shaders : Resource
        {
            public static Layout layout;

            public static Shader shadow_VS;
            public static Shader shadow_GS;

            public static Shader diffuse_VS;
            public static Shader diffuse_PS;

            protected override void Load()
            {
                layout = HLSL.CompileLayout("Shaders/shadow", ShaderStage.Vertex);

                shadow_VS = HLSL.CompileShader("Shaders/shadow", ShaderStage.Vertex);
                shadow_GS = HLSL.CompileShader("Shaders/shadow", ShaderStage.Geometry);

                diffuse_VS = HLSL.CompileShader("Shaders/diffuse", ShaderStage.Vertex);
                diffuse_PS = HLSL.CompileShader("Shaders/diffuse", ShaderStage.Pixel);
            }
            protected override void Dispose()
            {
                layout.Dispose();

                shadow_VS.Dispose();
                shadow_GS.Dispose();

                diffuse_VS.Dispose();
                diffuse_PS.Dispose();

                layout = null;

                shadow_VS = null;
                shadow_GS = null;

                diffuse_VS = null;
                diffuse_PS = null;
            }
        }
    }
}
