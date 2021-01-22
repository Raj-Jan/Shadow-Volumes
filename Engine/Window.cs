using System;
using System.Windows.Forms;
using System.Threading.Tasks;

using SharpDX.DirectInput;

using Mouse1 = SharpDX.DirectInput.Mouse;
using Keyboard1 = SharpDX.DirectInput.Keyboard;

namespace Engine
{
    public interface IKeyboard
    {
        bool IsKey(Key key, KeyState state);
    }

    public interface IMouse
    {
        Vector Position { get; }
        Vector Velocity { get; }

        bool IsButton(Button button, KeyState state);
    }

    public abstract class Window : Form, IKeyboard, IMouse
    {
        public Window()
        {
            using (var input = new DirectInput())
            {
                keyboard = new Keyboard1(input);
                keyboardState = new KeyboardState();
                keys = new KeyState[238];

                mouse = new Mouse1(input);
                mouseState = new MouseState();
                buttons = new KeyState[8];
            }

            keyboard.Acquire();
            mouse.Acquire();
        }

        private bool gotFocus;

        private Keyboard1 keyboard;
        private KeyboardState keyboardState;
        private KeyState[] keys;

        private Mouse1 mouse;
        private MouseState mouseState;
        private KeyState[] buttons;

        public Vector Position
        {
            get;
            private set;
        }
        public Vector Velocity
        {
            get => new Vector(mouseState.Y, mouseState.X, -mouseState.Z / 120);
        }

        public bool IsKey(Key key, KeyState state)
        {
            return keys[(int)key] == state;
        }
        public bool IsButton(Button button, KeyState state)
        {
            return buttons[(int)button] == state;
        }
        public void UpdateInput()
        {
            if (!Focused) return;

            keyboard.GetCurrentState(ref keyboardState);
            mouse.GetCurrentState(ref mouseState);

            foreach (var key in keyboardState.AllKeys)
            {
                var k = (int)key;

                if (keyboardState.PressedKeys.Contains(key))
                {
                    if (keys[k] == KeyState.JustPressed) keys[k] = KeyState.Pressed;
                    else if (keys[k] != KeyState.Pressed) keys[k] = KeyState.JustPressed;
                }
                else
                {
                    if (keys[k] == KeyState.JustRelesed) keys[k] = KeyState.Relesed;
                    else if (keys[k] != KeyState.Relesed) keys[k] = KeyState.JustRelesed;
                }
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                var current = mouseState.Buttons[i];

                if (current)
                {
                    if (buttons[i] == KeyState.JustPressed) buttons[i] = KeyState.Pressed;
                    else if (buttons[i] != KeyState.Pressed) buttons[i] = KeyState.JustPressed;
                }
                else
                {
                    if (buttons[i] == KeyState.JustRelesed) buttons[i] = KeyState.Relesed;
                    else if (buttons[i] != KeyState.Relesed) buttons[i] = KeyState.JustRelesed;
                }
            }

            if (gotFocus)
            {
                mouseState.X = 0;
                mouseState.Y = 0;
                mouseState.Z = 0;

                gotFocus = false;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            gotFocus = true;
        }
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (Disposing) return;

            keys.Clear();
            buttons.Clear();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var x = (float)(e.X << 1) / (Width - 1) - 1;
            var y = (float)(e.Y << 1) / (Height - 1) - 1;

            Position = new Vector(x, -y, Position.Z);
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            var delta = e.Delta / 120;

            Position = new Vector(Position.X, Position.Y, delta);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (keyboard == null) return;

            keyboard.Unacquire();
            keyboard.Dispose();

            mouse.Unacquire();
            mouse.Dispose();

            keyboard = null;
            keyboardState = null;

            mouse = null;
            mouseState = null;

            keys = null;
            buttons = null;
        }
    }

    public sealed class CoreWindow : Window
    {
        public CoreWindow()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = Screen.PrimaryScreen.Bounds;
            Opacity = 0;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            ShowAsync();
        }
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (Disposing) return;

            Opacity = 0;
        }

        private async void ShowAsync()
        {
            while (Opacity < 1)
            {
                Opacity += 0.1f;
                await Task.Delay(20);
            }
        }
    }

    public enum KeyState : byte
    {
        Relesed,
        JustPressed,
        Pressed,
        JustRelesed
    }

    public enum Key : int
    {
        Unknown = 0,
        Escape = 1,
        D1 = 2,
        D2 = 3,
        D3 = 4,
        D4 = 5,
        D5 = 6,
        D6 = 7,
        D7 = 8,
        D8 = 9,
        D9 = 10,
        D0 = 11,
        Minus = 12,
        Equals = 13,
        Back = 14,
        Tab = 15,
        Q = 16,
        W = 17,
        E = 18,
        R = 19,
        T = 20,
        Y = 21,
        U = 22,
        I = 23,
        O = 24,
        P = 25,
        LeftBracket = 26,
        RightBracket = 27,
        Return = 28,
        LeftControl = 29,
        A = 30,
        S = 31,
        D = 32,
        F = 33,
        G = 34,
        H = 35,
        J = 36,
        K = 37,
        L = 38,
        Semicolon = 39,
        Apostrophe = 40,
        Grave = 41,
        LeftShift = 42,
        Backslash = 43,
        Z = 44,
        X = 45,
        C = 46,
        V = 47,
        B = 48,
        N = 49,
        M = 50,
        Comma = 51,
        Period = 52,
        Slash = 53,
        RightShift = 54,
        Multiply = 55,
        LeftAlt = 56,
        Space = 57,
        Capital = 58,
        F1 = 59,
        F2 = 60,
        F3 = 61,
        F4 = 62,
        F5 = 63,
        F6 = 64,
        F7 = 65,
        F8 = 66,
        F9 = 67,
        F10 = 68,
        NumberLock = 69,
        ScrollLock = 70,
        NumberPad7 = 71,
        NumberPad8 = 72,
        NumberPad9 = 73,
        Subtract = 74,
        NumberPad4 = 75,
        NumberPad5 = 76,
        NumberPad6 = 77,
        Add = 78,
        NumberPad1 = 79,
        NumberPad2 = 80,
        NumberPad3 = 81,
        NumberPad0 = 82,
        Decimal = 83,
        Oem102 = 86,
        F11 = 87,
        F12 = 88,
        F13 = 100,
        F14 = 101,
        F15 = 102,
        Kana = 112,
        AbntC1 = 115,
        Convert = 121,
        NoConvert = 123,
        Yen = 125,
        AbntC2 = 126,
        NumberPadEquals = 141,
        PreviousTrack = 144,
        AT = 145,
        Colon = 146,
        Underline = 147,
        Kanji = 148,
        Stop = 149,
        AX = 150,
        Unlabeled = 151,
        NextTrack = 153,
        NumberPadEnter = 156,
        RightControl = 157,
        Mute = 160,
        Calculator = 161,
        PlayPause = 162,
        MediaStop = 164,
        VolumeDown = 174,
        VolumeUp = 176,
        WebHome = 178,
        NumberPadComma = 179,
        Divide = 181,
        PrintScreen = 183,
        RightAlt = 184,
        Pause = 197,
        Home = 199,
        Up = 200,
        PageUp = 201,
        Left = 203,
        Right = 205,
        End = 207,
        Down = 208,
        PageDown = 209,
        Insert = 210,
        Delete = 211,
        LeftWindowsKey = 219,
        RightWindowsKey = 220,
        Applications = 221,
        Power = 222,
        Sleep = 223,
        Wake = 227,
        WebSearch = 229,
        WebFavorites = 230,
        WebRefresh = 231,
        WebStop = 232,
        WebForward = 233,
        WebBack = 234,
        MyComputer = 235,
        Mail = 236,
        MediaSelect = 237
    }

    public enum Button : byte
    {
        Left,
        Right,
        Middle,
        X1,
        X2,
        X3,
        X4,
        X5
    }
}
