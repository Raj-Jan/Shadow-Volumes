using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using SharpDX.DXGI;
using SharpDX.Win32;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using Message = System.Windows.Forms.Message;
using Device = SharpDX.Direct3D11.Device;

using static Engine.Graphics;
using DeviceChild = SharpDX.Direct3D11.DeviceChild;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Engine
{
    public interface ITime
    {
        uint Frame { get; }
        float Elapsed { get; }
        float Total { get; }
    }

    [Resource]
    public abstract class Resource : ILoad
    {
        private bool disposed = true;

        protected abstract void Load();
        protected abstract void Dispose();

        void ILoad.Load()
        {
            if (!disposed) return;
            Load();
            disposed = false;
        }
        void IDisposable.Dispose()
        {
            if (disposed) return;
            Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    public static class Graphics
    {
        static Graphics()
        {
            device = new Device(DriverType.Hardware);
            context = device.ImmediateContext;

            AppDomain.CurrentDomain.ProcessExit += Dispose;
        }

        internal readonly static Device device;
        internal readonly static DeviceContext context;

        private static void Dispose(object sender, EventArgs args)
        {
            device.Dispose();
        }
    }

    public abstract class Core1 : Disposable
    {
        private static Core1 instance;

        public static IKeyboard Keyboard
        {
            get => instance.window;
        }
        public static IMouse Mouse
        {
            get => instance.window;
        }

        public Core1()
        {
            timer = new Timer();
            window = new CoreWindow();

            using (var factory = new Factory1())
            using (var adapter = factory.GetAdapter1(0))
            using (var output = adapter.GetOutput(0))
            {
                var modes = output.GetDisplayModeList(Format.B8G8R8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);
                var mode = modes[^1];

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
        }

        private bool active;

        private readonly Timer timer;
        private readonly Window window;
        private readonly SwapChain swapChain;
        private readonly RenderTargetView target;

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

        protected virtual void Initialize()
        {
            active = true;
            window.Show();
            timer.Start();
        }
        protected virtual void Update(ITime time)
        {
            
        }
        protected virtual void Draw()
        {
            context.OutputMerger.SetTargets(target);
            context.Draw(4, 0);
        }

        protected override void Dispose(bool disposing)
        {
            window.Dispose();
            swapChain.Dispose();
            target.Dispose();
        }

        private void Frame()
        {
            Update(timer);
            Draw();
            Process();

            swapChain.Present(1, PresentFlags.None);
            window.UpdateInput();
            timer.Update();
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
    }

    public static class PostEffects
    {
        static PostEffects()
        {
            byte[] vsCode = 
                { 68, 88, 66, 67, 161, 8, 199, 80, 208, 248, 48, 57, 214, 8, 148, 111, 248, 124, 180, 193, 1, 0, 0, 0, 124, 2, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 160, 0, 0, 0, 244, 0, 0, 0, 76, 1, 0, 0, 224, 1, 0, 0, 82, 68, 69, 70, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 60, 0, 0, 0, 0, 5, 254, 255, 0, 1, 0, 0, 60, 0, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 73, 83, 71, 78, 76, 0, 0, 0, 2, 0, 0, 0, 8, 0, 0, 0, 56, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 3, 3, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 3, 3, 0, 0, 80, 79, 83, 73, 84, 73, 79, 78, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 79, 83, 71, 78, 80, 0, 0, 0, 2, 0, 0, 0, 8, 0, 0, 0, 56, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 68, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 3, 12, 0, 0, 83, 86, 95, 80, 111, 115, 105, 116, 105, 111, 110, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 171, 83, 72, 69, 88, 140, 0, 0, 0, 80, 0, 1, 0, 35, 0, 0, 0, 106, 8, 0, 1, 95, 0, 0, 3, 50, 16, 16, 0, 0, 0, 0, 0, 95, 0, 0, 3, 50, 16, 16, 0, 1, 0, 0, 0, 103, 0, 0, 4, 242, 32, 16, 0, 0, 0, 0, 0, 1, 0, 0, 0, 101, 0, 0, 3, 50, 32, 16, 0, 1, 0, 0, 0, 54, 0, 0, 5, 50, 32, 16, 0, 0, 0, 0, 0, 70, 16, 16, 0, 0, 0, 0, 0, 54, 0, 0, 8, 194, 32, 16, 0, 0, 0, 0, 0, 2, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 54, 0, 0, 5, 50, 32, 16, 0, 1, 0, 0, 0, 70, 16, 16, 0, 1, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  };
            byte[] psCompose = 
                { 68, 88, 66, 67, 49, 255, 97, 245, 91, 109, 81, 138, 229, 205, 21, 25, 83, 32, 85, 244, 1, 0, 0, 0, 144, 2, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 240, 0, 0, 0, 72, 1, 0, 0, 124, 1, 0, 0, 244, 1, 0, 0, 82, 68, 69, 70, 180, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 60, 0, 0, 0, 0, 5, 255, 255, 0, 1, 0, 0, 137, 0, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 124, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 133, 0, 0, 0, 2, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 1, 0, 0, 0, 13, 0, 0, 0, 95, 115, 97, 109, 112, 108, 101, 114, 0, 116, 101, 120, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 171, 171, 171, 73, 83, 71, 78, 80, 0, 0, 0, 2, 0, 0, 0, 8, 0, 0, 0, 56, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 68, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 3, 3, 0, 0, 83, 86, 95, 80, 111, 115, 105, 116, 105, 111, 110, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 171, 79, 83, 71, 78, 44, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 83, 86, 95, 84, 97, 114, 103, 101, 116, 0, 171, 171, 83, 72, 69, 88, 112, 0, 0, 0, 80, 0, 0, 0, 28, 0, 0, 0, 106, 8, 0, 1, 90, 0, 0, 3, 0, 96, 16, 0, 0, 0, 0, 0, 88, 24, 0, 4, 0, 112, 16, 0, 0, 0, 0, 0, 85, 85, 0, 0, 98, 16, 0, 3, 50, 16, 16, 0, 1, 0, 0, 0, 101, 0, 0, 3, 242, 32, 16, 0, 0, 0, 0, 0, 69, 0, 0, 139, 194, 0, 0, 128, 67, 85, 21, 0, 242, 32, 16, 0, 0, 0, 0, 0, 70, 16, 16, 0, 1, 0, 0, 0, 70, 126, 16, 0, 0, 0, 0, 0, 0, 96, 16, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  };
            byte[] psFxaa = 
                { 68, 88, 66, 67, 49, 255, 97, 245, 91, 109, 81, 138, 229, 205, 21, 25, 83, 32, 85, 244, 1, 0, 0, 0, 144, 2, 0, 0, 5, 0, 0, 0, 52, 0, 0, 0, 240, 0, 0, 0, 72, 1, 0, 0, 124, 1, 0, 0, 244, 1, 0, 0, 82, 68, 69, 70, 180, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 60, 0, 0, 0, 0, 5, 255, 255, 0, 1, 0, 0, 137, 0, 0, 0, 82, 68, 49, 49, 60, 0, 0, 0, 24, 0, 0, 0, 32, 0, 0, 0, 40, 0, 0, 0, 36, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 124, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 133, 0, 0, 0, 2, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 1, 0, 0, 0, 13, 0, 0, 0, 95, 115, 97, 109, 112, 108, 101, 114, 0, 116, 101, 120, 0, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 40, 82, 41, 32, 72, 76, 83, 76, 32, 83, 104, 97, 100, 101, 114, 32, 67, 111, 109, 112, 105, 108, 101, 114, 32, 49, 48, 46, 49, 0, 171, 171, 171, 73, 83, 71, 78, 80, 0, 0, 0, 2, 0, 0, 0, 8, 0, 0, 0, 56, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 68, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 0, 3, 3, 0, 0, 83, 86, 95, 80, 111, 115, 105, 116, 105, 111, 110, 0, 84, 69, 88, 67, 79, 79, 82, 68, 0, 171, 171, 171, 79, 83, 71, 78, 44, 0, 0, 0, 1, 0, 0, 0, 8, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 83, 86, 95, 84, 97, 114, 103, 101, 116, 0, 171, 171, 83, 72, 69, 88, 112, 0, 0, 0, 80, 0, 0, 0, 28, 0, 0, 0, 106, 8, 0, 1, 90, 0, 0, 3, 0, 96, 16, 0, 0, 0, 0, 0, 88, 24, 0, 4, 0, 112, 16, 0, 0, 0, 0, 0, 85, 85, 0, 0, 98, 16, 0, 3, 50, 16, 16, 0, 1, 0, 0, 0, 101, 0, 0, 3, 242, 32, 16, 0, 0, 0, 0, 0, 69, 0, 0, 139, 194, 0, 0, 128, 67, 85, 21, 0, 242, 32, 16, 0, 0, 0, 0, 0, 70, 16, 16, 0, 1, 0, 0, 0, 70, 126, 16, 0, 0, 0, 0, 0, 0, 96, 16, 0, 0, 0, 0, 0, 62, 0, 0, 1, 83, 84, 65, 84, 148, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };

            var vertices = new Color[]
            {
                new Color(-1,  1, 0, 0),
                new Color(-1, -1, 0, 1),
                new Color( 1,  1, 1, 0),
                new Color( 1, -1, 1, 1),
            };

            var buffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);

            var elements = new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float, -1, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, -1, 0, InputClassification.PerVertexData, 0)
            };
            var rasterizerDesc = new RasterizerStateDescription()
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
            var stencilDesc = new DepthStencilStateDescription
            {
                IsDepthEnabled = false,
                IsStencilEnabled = false,
            };
            var equalDesc = new DepthStencilStateDescription
            {
                IsDepthEnabled = false,
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
            var blendDesc = new BlendStateDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };
            var additiveDesc = new BlendStateDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };

            var blendTargetDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,

                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };
            var additiveTargetDesc = new RenderTargetBlendDescription
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

            blendDesc.RenderTarget[0] = blendTargetDesc;
            additiveDesc.RenderTarget[0] = additiveTargetDesc;

            quad = new VertexBufferBinding(buffer, 16, 0);
            layout = new InputLayout(device, vsCode, elements);
            vs = new VertexShader(device, vsCode);
            compose = new PixelShader(device, psCompose);
            fxaa = new PixelShader(device, psFxaa);

            rasterizer = new RasterizerState(device, rasterizerDesc);
            stencil = new DepthStencilState(device, stencilDesc);
            equal = new DepthStencilState(device, equalDesc);

            blend = new BlendState(device, blendDesc);
            additive = new BlendState(device, additiveDesc);

            AppDomain.CurrentDomain.ProcessExit += Dispose;
        }

        private static readonly RasterizerState rasterizer;
        private static readonly DepthStencilState stencil;
        private static readonly DepthStencilState equal;
        private static readonly BlendState additive;
        private static readonly BlendState blend;

        private static readonly VertexBufferBinding quad;
        private static readonly InputLayout layout;
        private static readonly DeviceChild vs;
        private static readonly DeviceChild compose;
        private static readonly DeviceChild fxaa;

        public static void Begin()
        {
            context.Rasterizer.State = rasterizer;

            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            context.InputAssembler.SetVertexBuffers(0, quad);

            context.VertexShader.SetShader(vs, null, 0);
        }
        public static void Combine()
        {
            context.OutputMerger.SetDepthStencilState(equal);
            context.OutputMerger.SetBlendState(additive);

            context.PixelShader.SetShader(compose, null, 0);
            context.Draw(4, 0);
        }
        public static void FXAA()
        {
            context.OutputMerger.SetDepthStencilState(stencil);
            context.OutputMerger.SetBlendState(blend);

            context.PixelShader.SetShader(fxaa, null, 0);
            context.OutputMerger.ResetTargets();
            //context.Draw(4, 0);
        }

        private static void Dispose(object sender, EventArgs args)
        {
            rasterizer.Dispose();
            stencil.Dispose();
            equal.Dispose();
            additive.Dispose();
            blend.Dispose();

            quad.Buffer.Dispose();
            layout.Dispose();
            vs.Dispose();
            compose.Dispose();
            fxaa.Dispose();
        }
    }

    public class Lighting : Resource
    {
        private static Buffer buffer1;
        private static Buffer buffer2;

        private static InputLayout layout;
        private static DeviceChild lighting_VS;
        private static DeviceChild lighting_PS;
        private static DeviceChild shadow_VS;
        private static DeviceChild shadow_GS;

        public static void Light()
        {
            context.InputAssembler.InputLayout = layout;

            context.VertexShader.SetShader(lighting_VS, null, 0);
            context.GeometryShader.Set(null);
            context.PixelShader.SetShader(lighting_PS, null, 0);
        }
        public static void Shadow()
        {
            context.VertexShader.SetShader(shadow_VS, null, 0);
            context.GeometryShader.SetShader(shadow_GS, null, 0);
            context.PixelShader.Set(null);
        }

        public static void SetValue(Matrix transform)
        {
            context.UpdateSubresource(ref transform, buffer1);
            context.VertexShader.SetConstantBuffer(0, buffer1);
        }
        public static void SetValue(Matrix transform, Vector dir)
        {
            var data = new Data
            {
                world = transform,
                dir = ~transform * dir
            };

            context.UpdateSubresource(ref data, buffer1);
            context.VertexShader.SetConstantBuffer(0, buffer1);
        }
        public static void SetValue(Color ambient, Color diffuse)
        {
            var data = new Data1
            {
                ambient = ambient,
                diffuse = diffuse
            };

            context.UpdateSubresource(ref data, buffer2);
            context.PixelShader.SetConstantBuffer(0, buffer2);
        }
        public static void SetValue(Color extension)
        {
            context.UpdateSubresource(ref extension, buffer2);
            context.GeometryShader.SetConstantBuffer(0, buffer2);
        }

        protected override void Load()
        {
            var desc = new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            };

            desc.SizeInBytes = 80;
            buffer1 = new Buffer(device, desc);
            desc.SizeInBytes = 32;
            buffer2 = new Buffer(device, desc);
        }
        protected override void Dispose()
        {
            buffer1.Dispose();
            buffer2.Dispose();

            layout.Dispose();
            lighting_VS.Dispose();
            lighting_PS.Dispose();
            shadow_VS.Dispose();
            shadow_GS.Dispose();
        }

        private struct Data
        {
            public Matrix world { get; init; }
            public Vector dir { get; init; }
        }

        private struct Data1
        {
            public Color ambient { get; init; }
            public Color diffuse { get; init; }
        }
    }

    public class States : Resource
    {
        static States()
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

            frontCull = new RasterizerState(device, desc1);
            noCull = new RasterizerState(device, desc2);

            blend = new BlendState(device, desc5);
            additive = new BlendState(device, desc6);

            none = new DepthStencilState(device, desc7);
            depth = new DepthStencilState(device, desc8);
            equal = new DepthStencilState(device, desc9);
            incdec = new DepthStencilState(device, desc10);
        }

        private static RasterizerState frontCull;
        private static RasterizerState noCull;

        private static DepthStencilState none;
        private static DepthStencilState depth;
        private static DepthStencilState equal;
        private static DepthStencilState incdec;

        private static BlendState blend;
        private static BlendState additive;

        public static void Light()
        {
            context.Rasterizer.State = frontCull;
            context.OutputMerger.SetDepthStencilState(depth);
            context.OutputMerger.SetBlendState(blend);
        }
        public static void Shadow()
        {
            context.Rasterizer.State = noCull;
            context.OutputMerger.SetDepthStencilState(incdec);
            context.OutputMerger.SetBlendState(null);
        }
        public static void Composite()
        {
            context.Rasterizer.State = noCull;
            context.OutputMerger.SetDepthStencilState(equal);
            context.OutputMerger.SetBlendState(additive);
        }
        public static void FXAA()
        {
            context.OutputMerger.SetDepthStencilState(none);
            context.OutputMerger.SetBlendState(blend);
        }

        protected override void Load()
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
                DepthWriteMask = DepthWriteMask.All,
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

            frontCull = new RasterizerState(device, desc1);
            noCull = new RasterizerState(device, desc2);

            blend = new BlendState(device, desc5);
            additive = new BlendState(device, desc6);

            none = new DepthStencilState(device, desc7);
            depth = new DepthStencilState(device, desc8);
            equal = new DepthStencilState(device, desc9);
            incdec = new DepthStencilState(device, desc10);
        }
        protected override void Dispose()
        {
            frontCull.Dispose();
            noCull.Dispose();

            none.Dispose();
            depth.Dispose();
            equal.Dispose();
            incdec.Dispose();

            blend.Dispose();
            additive.Dispose();
        }
    }

    public struct Light
    {
        public Color Ambient { get; set; }
        public Color Diffuse { get; set; }
        public Vector Direction { get; set; }
    }

    public class Compositor : Disposable
    {
        public Compositor(int width, int height)
        {
            var desc = new Texture2DDescription
            {
                Width = width,
                Height = height,
                ArraySize = 1,
                MipLevels = 1,
                Format = Format.B8G8R8A8_UNorm,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
            };

            using (var tex = new Texture2D(device, desc))
            {
                ambient = new RenderTargetView(device, tex);
                _ambient = new ShaderResourceView(device, tex);
            }

            using (var tex = new Texture2D(device, desc))
            {
                diffuse = new RenderTargetView(device, tex);
                _diffuse = new ShaderResourceView(device, tex);
            }

            desc.Format = Format.D24_UNorm_S8_UInt;
            desc.BindFlags = BindFlags.DepthStencil;

            using (var tex = new Texture2D(device, desc))
                stencil = new DepthStencilView(device, tex);
        }

        private readonly DepthStencilView stencil;
        private readonly RenderTargetView ambient;
        private readonly RenderTargetView diffuse;

        private readonly ShaderResourceView _ambient;
        private readonly ShaderResourceView _diffuse;

        public void Commit()
        {
            States.Composite();
            PostEffects.Combine();

            context.PixelShader.SetShaderResource(0, _diffuse);
            context.OutputMerger.SetRenderTargets(ambient);
            context.Draw(4, 0);

            States.FXAA();
            PostEffects.FXAA();

            context.PixelShader.SetShaderResource(0, _ambient);
        }
        public void DoLight()
        {
            Shaders.Light();
            States.Light();
        }
        public void DoShadow()
        {
            Shaders.Shadow();
            States.Shadow();
        }

        public void SetValue(Matrix camera, Matrix world, Vector direction)
        {
            Shaders.SetValue(camera * world, direction);
        }
        public void SetValue(Matrix camera, Matrix world)
        {
            Shaders.SetValue(camera * world);
        }
        public void SetValue(Matrix camera, Vector direction)
        {
            Shaders.SetValue(camera * new Color(direction, 0));
        }
        public void SetValue(Color ambient, Color diffuse)
        {
            Shaders.SetValue(ambient, diffuse);
        }

        protected override void Dispose(bool disposing)
        {
            stencil.Dispose();
            ambient.Dispose();
            diffuse.Dispose();

            _ambient.Dispose();
            _diffuse.Dispose();
        }

        private class Shaders : Resource
        {
            private static InputLayout layout;
            private static DeviceChild lighting_VS;
            private static DeviceChild lighting_PS;
            private static DeviceChild shadow_VS;
            private static DeviceChild shadow_GS;

            private static Buffer buffer1;
            private static Buffer buffer2;

            public static void Light()
            {
                context.InputAssembler.InputLayout = layout;

                context.VertexShader.SetShader(lighting_VS, null, 0);
                context.GeometryShader.Set(null);
                context.PixelShader.SetShader(lighting_PS, null, 0);
            }
            public static void Shadow()
            {
                context.VertexShader.SetShader(shadow_VS, null, 0);
                context.GeometryShader.SetShader(shadow_GS, null, 0);
                context.PixelShader.Set(null);
            }

            public static void SetValue(Matrix transform, Vector dir)
            {
                var data = new Data
                {
                    world = transform,
                    dir = ~transform * dir
                };

                context.UpdateSubresource(ref data, buffer1);
                context.VertexShader.SetConstantBuffer(0, buffer1);
            }
            public static void SetValue(Color ambient, Color diffuse)
            {
                var data = new Data1
                {
                    ambient = ambient,
                    diffuse = diffuse
                };

                context.UpdateSubresource(ref data, buffer2);
                context.PixelShader.SetConstantBuffer(0, buffer2);
            }
            public static void SetValue(Matrix transform)
            {
                context.UpdateSubresource(ref transform, buffer1);
                context.VertexShader.SetConstantBuffer(0, buffer1);
            }
            public static void SetValue(Color direction)
            {
                context.UpdateSubresource(ref direction, buffer2);
                context.GeometryShader.SetConstantBuffer(0, buffer2);
            }

            protected override void Load()
            {
                
            }
            protected override void Dispose()
            {
                layout.Dispose();
                lighting_VS.Dispose();
                lighting_PS.Dispose();
                shadow_VS.Dispose();
                shadow_GS.Dispose();

                buffer1.Dispose();
                buffer2.Dispose();
            }

            private struct Data
            {
                public Matrix world { get; init; }
                public Vector dir { get; init; }
            }

            private struct Data1
            {
                public Color ambient { get; init; }
                public Color diffuse { get; init; }
            }
        }
    }
}
