using System.Threading;

using Engine;
using static Engine.Core;

namespace Game
{
    internal class Program : Core
    {
        private static void Main()
        {
            using (var platform = new Program())
                platform.Run();
        }

        protected override void Initialize()
        {
            SetScene<Loader>();
            base.Initialize();
        }
    }

    public class Loader : Scene
    {
        public Loader()
        {
            resources = new Library<ILoad>();
        }

        private readonly Library<ILoad> resources;

        public override void Initialize()
        {
            resources.Gather<PriorityAttribute>(x => x.Load());

            var thread = new Thread(InitializeAsync)
            {
                IsBackground = true
            };

            thread.Start();
        }
        public override void Update(ITime time)
        {

        }
        public override void Draw()
        {

        }

        protected virtual void InitializeAsync()
        {
            resources.Gather<ResourceAttribute>(OnLoad);

            SetScene<WorldScene>();
        }
        protected virtual void OnLoad(ILoad resource)
        {
            resource.Load();
        }

        protected override void Dispose(bool disposing)
        {
            resources.Dispose();
        }
    }

    public class WorldScene : World
    {
        private RenderView render;
        private ShaderView shader;

        private DirectionalLight light;
        private Camera camera;

        public override void Initialize()
        {
            render = new RenderView(1920, 1080, 2);
            shader = new ShaderView(2);

            render.Fill(shader);

            light = new DirectionalLight()
            {
                Ambient = new Color(0.1f, 0.1f, 0.1f),
                Diffuse = new Color(1f, 1f, 1f),
                Direction = new Vector(-1, -1, -3).Normalize()
            };
            camera = new Camera(new Vector(5, 5, 5));

            Add(new Cube(0, 0, 2.8f));
            Add(new Terrain());
        }
        public override void Update(ITime time)
        {
            if (Keyboard.IsKey(Key.Escape, KeyState.JustRelesed)) Exit();

            camera.Update(time);

            base.Update(time);
        }
        public override void Draw()
        {
            render.Clear(1, 0);
            render.Clear(0, new Color(0.2f, 0.2f, 0.2f));
            render.Clear(1, new Color(0, 0, 0));

            render.SetAsTarget();

            DirectionalLight.Begin();

            light.DrawDiffuse(camera, this);
            Shader.ClearPS();

            render.SetAsTargetDepthStencilOnly();
            light.DrawShadow(camera, this);

            render.SetAsTarget(0);

            Shader.ClearGS();

            PostEffects.Begin();
            shader.PassToPS(1);
            PostEffects.Combine();
            PostEffects.FXAA();
            shader.PassToPS(0);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
