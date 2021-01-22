using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using SharpDX.DXGI;
using SharpDX.Win32;
using SharpDX.Direct3D11;

using Message = System.Windows.Forms.Message;

using static Engine.Graphics;
using System.Collections.Generic;

namespace Engine
{
    public abstract class Core : Disposable
    {
        private static Core instance;

        public static IKeyboard Keyboard
        {
            get => instance.window;
        }
        public static IMouse Mouse
        {
            get => instance.window;
        }

        public Core()
        {
            timer = new Timer();
            window = new CoreWindow();
            scenes = new Library<Scene>();

            using (var factory = new Factory1())
            using (var adapter = factory.GetAdapter1(0))
            using (var output = adapter.GetOutput(0))
            {
                var modes = output.GetDisplayModeList(Format.B8G8R8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);
                var mode = modes[^1];

                Width = mode.Width;
                Height = mode.Height;

                var desc = new SwapChainDescription()
                {
                    OutputHandle = window.Handle,
                    ModeDescription = mode,
                    BufferCount = 1,
                    IsWindowed = true,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.RenderTargetOutput,
                    SwapEffect = SwapEffect.Discard,
                    Flags = SwapChainFlags.None,
                };

                swapChain = new SwapChain(factory, device, desc);

                using (var tex = swapChain.GetBackBuffer<Texture2D>(0))
                    target = new RenderTargetView(device, tex);
            }

            scenes.Gather<SceneAttribute>();
        }

        private bool active;
        private Scene current;
        private Scene pending;

        private readonly Timer timer;
        private readonly Window window;
        private readonly SwapChain swapChain;
        private readonly RenderTargetView target;
        private readonly Library<Scene> scenes;

        protected int Width { get; }
        protected int Height { get; }

        protected bool IsCursorVisible
        {
            set
            {
                if (value) Cursor.Show();
                else Cursor.Hide();
            }
        }

        public void Run()
        {
            if (instance is not null) return;

            instance = this;
            Initialize();

            while (active)
                Frame();

            timer.Stop();
            window.Hide();
            instance = null;
        }
        public void Exit()
        {
            active = false;
        }

        protected void Create(out RenderTargetView target)
        {
            using (var texture = swapChain.GetBackBuffer<Texture2D>(0))
                target = new RenderTargetView(device, texture);
        }
        protected void Bind(RenderView render)
        {
            using (var texture = swapChain.GetBackBuffer<Texture2D>(0))
                render.Init(texture);
        }
        protected void SetScene<T>() where T : Scene
        {
            foreach (var scene in scenes)
            {
                if (scene is T)
                {
                    pending = scene;
                    break;
                }
            }
        }

        protected virtual void Initialize()
        {
            active = true;
            window.Show();
            timer.Start();
        }
        protected virtual void Update(ITime time)
        {
            current.Update(time);
        }
        protected virtual void Draw()
        {
            current.Draw();
        }

        protected override void Dispose(bool disposing)
        {
            window.Dispose();
            swapChain.Dispose();
            scenes.Dispose();
            target.Dispose();
        }

        private void Frame()
        {
            Swap();
            Update(timer);
            Draw();
            Process();

            context.OutputMerger.SetTargets(target);
            context.Draw(4, 0);

            swapChain.Present(1, PresentFlags.None);
            window.UpdateInput();
            timer.Update();
        }
        private void Swap()
        {
            if (pending == null) return;

            pending.Initialize();

            current = pending;
            pending = null;
        }
        private void Process()
        {
            NativeMessage msg;
            while (Utils.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0) != 0)
            {
                if (Utils.GetMessage(out msg, IntPtr.Zero, 0, 0) == -1)
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new Exception($"Error code {error} occured while prcessing messages.");
                }

                if (msg.msg == 0x0112) active = msg.wParam.ToInt32() != 0x0000f060;
                if (msg.msg == 0x00a1) active = msg.wParam.ToInt32() != 0x00000014;

                var message = new Message()
                {
                    HWnd = msg.handle,
                    LParam = msg.lParam,
                    Msg = (int)msg.msg,
                    WParam = msg.wParam
                };

                if (!Application.FilterMessage(ref message))
                {
                    Utils.TranslateMessage(ref msg);
                    Utils.DispatchMessage(ref msg);
                }
            }
        }

        [Scene]
        public abstract class Scene : Disposable
        {
            protected static void Bind(RenderView render)
            {
                instance.Bind(render);
            }
            protected static void Create(out RenderTargetView target)
            {
                instance.Create(out target);
            }
            protected static void Create(int count, out RenderView render, out ShaderView shader)
            {
                render = new RenderView(instance.Width, instance.Height, count + 1);
                shader = new ShaderView(count);

                Bind(render);
                render.Fill(shader);
            }
            protected static void SetScene<T>() where T : Scene
            {
                instance.SetScene<T>();
            }
            protected static void Exit()
            {
                instance.Exit();
            }

            public abstract void Initialize();
            public abstract void Update(ITime time);
            public abstract void Draw();
        }
    }

    public class Timer : ITime
    {
        public Timer()
        {
            stopwatch = new Stopwatch();
        }

        private readonly Stopwatch stopwatch;

        public uint Frame { get; set; }
        public float Elapsed { get; set; }
        public float Total { get; set; }

        public void Start()
        {
            stopwatch.Start();
        }
        public void Stop()
        {
            stopwatch.Stop();
        }
        public void Reset()
        {
            stopwatch.Reset();
        }
        public void Update()
        {
            var current = (float)stopwatch.Elapsed.TotalSeconds;
            Elapsed = current - Total;
            Total = current;
            Frame++;
        }
    }
}
