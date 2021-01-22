using System;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SharpDX.Win32;

namespace Engine
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ResourceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class SceneAttribute : Attribute { }

    public interface ILoad : IDisposable
    {
        void Load();
    }

    public abstract class Disposable : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed) return;
            Dispose(true);
            disposed = true;
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }

    public class Library<T> : IEnumerable<T> where T : class
    {
        public Library()
        {
            resources = new List<T>();
        }

        private readonly List<T> resources;

        public void Clear()
        {
            resources.Clear();
        }

        public void Gather<V>() where V : Attribute
        {
            var assembly = Assembly.GetEntryAssembly();
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (Contains(type)) continue;
                if (type.GetCustomAttribute<V>() == null) continue;
                if (type.IsGenericType) continue;
                if (type.IsAbstract) continue;

                var resource = Activator.CreateInstance(type, true) as T;

                if (resource != null)
                    resources.Add(resource);
            }
        }
        public void Gather<V>(Action<T> onGather) where V : Attribute
        {
            var assembly = Assembly.GetEntryAssembly();
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (Contains(type)) continue;
                if (type.GetCustomAttribute<V>() == null) continue;
                if (type.IsGenericType) continue;
                if (type.IsAbstract) continue;

                var resource = Activator.CreateInstance(type, true) as T;

                if (resource is not null)
                {
                    onGather(resource);
                    resources.Add(resource);
                }
            }
        }

        private bool Contains(Type type)
        {
            foreach (var resource in resources)
                if (resource.GetType() == type)
                    return true;

            return false;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return resources.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return resources.GetEnumerator();
        }
    }

    public static class Utils
    {
        public const float PI = (float)Math.PI;
        public const float D120 = (float)(Math.PI / 1.5);
        public const float D90 = (float)(Math.PI / 2);
        public const float D60 = (float)(Math.PI / 3);

        public static void Dispose<T>(this T[] disposables) where T : IDisposable
        {
            foreach (var disposable in disposables)
                disposable?.Dispose();

            disposables.Clear();
        }
        public static void Dispose<T>(this Library<T> disposables) where T : class, IDisposable
        {
            foreach (var disposable in disposables)
                disposable?.Dispose();

            disposables.Clear();
        }
        public static void Dispose<T>(this IEnumerable<T> disposables) where T : IDisposable
        {
            foreach (var disposable in disposables)
                disposable?.Dispose();
        }
        public static void Dispose<T, V>(this IDictionary<T, V> disposables) where V : IDisposable
        {
            foreach (var disposable in disposables.Values)
                disposable?.Dispose();

            disposables.Clear();
        }

        public static void Clear<T>(this T[] array)
        {
            if (array.Length < 77)
            {
                for (int i = 0; i < array.Length; i++)
                    array[i] = default;
            }
            else Array.Clear(array, 0, array.Length);
        }

        [DllImport("user32.dll")]
        internal static extern int PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgMin, int wMsgMax, int wRemoveMsg);
        [DllImport("user32.dll")]
        internal static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgMin, int wMsgMax);
        [DllImport("user32.dll")]
        internal static extern void TranslateMessage(ref NativeMessage msg);
        [DllImport("user32.dll")]
        internal static extern void DispatchMessage(ref NativeMessage msg);
    }

    public static class Debug
    {
        public static void Try(Action action, bool fatal = true)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Throw(e, fatal);
            }
        }
        public static void Throw(Exception e, bool fatal = true)
        {
            MessageBox.Show(e.ToString());

            if (fatal)
            {
                Application.Exit();
                Environment.Exit(0);
            }
        }
        public static void Throw(string s, bool fatal = true)
        {
            MessageBox.Show(s);

            if (fatal)
            {
                Application.Exit();
                Environment.Exit(0);
            }
        }
    }
}
