using Engine;

using static Engine.Core;

namespace Game
{
    public class Camera : DynamicBody, ICamera
    {
        public Camera(Vector position)
        {
            projection = Matrix.CreateProjection(Utils.D60, 9f / 16, 1, 40);
            World = Matrix.CreateLook(position, new Vector(0, 0, 0), new Vector(0, 0, 1));
        }

        private Matrix projection;

        Matrix ICamera.View => projection * World;

        public float Sensivity { get; set; } = 2;
        public float Inertia { get; set; } = 10;

        public override void Update(ITime time)
        {
            var acc = -Mouse.Velocity;
            acc.Z = 0;

            VelocityA += time.Elapsed * (Sensivity * ~World * acc - Inertia * VelocityA);

            base.Update(time);
        }

        public override void Draw()
        {

        }
        public override void Shape()
        {

        }
    }

    public class Cube : DynamicBody
    {
        public Cube(float x, float y, float z)
        {
            World = Matrix.CreateTranslation(x, y, z);

            VelocityA = new Vector(0, 0, 1);
        }

        public override void Draw()
        {
            Model.buffer.PassToPS();
            Model.mesh.Draw();
        }
        public override void Shape()
        {
            Model.mesh.Draw();   
        }

        private class Model : Resource
        {
            public static ShaderView buffer;
            public static Mesh mesh;

            protected override void Load()
            {
                buffer = new ShaderView(1);

                mesh = MDL.FromFile("Meshes/cube");

                PNG.FromFile(ref buffer, "Textures/texture");
            }
            protected override void Dispose()
            {
                mesh.Dispose();
                buffer.Dispose();

                mesh = null;
                buffer = null;
            }
        }
    }

    public class Terrain : World.Entity
    {
        public Terrain()
        {
            World = Matrix.CreateScale(10, 10, 10);
        }

        public override void Update(ITime time)
        {

        }
        public override void Draw()
        {
            Model.buffer.PassToPS();
            Model.mesh.Draw();
        }
        public override void Shape()
        {
            Model.mesh.Draw();
        }

        private class Model : Resource
        {
            public static ShaderView buffer;
            public static Mesh mesh;

            protected override void Load()
            {
                buffer = new ShaderView(1);

                mesh = MDL.FromFile("Meshes/terrain1");

                PNG.FromFile(ref buffer, "Textures/texture");
            }
            protected override void Dispose()
            {
                mesh.Dispose();
                buffer.Dispose();

                mesh = null;
                buffer = null;
            }
        }
    }
}
