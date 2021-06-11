using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;

namespace CobaCamera
{
    class Program
    {
        static void Main(string[] args)
        {
            var ourWindowSetting = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Pertemuan Camera"
            };
            using (var win = new Windows(GameWindowSettings.Default, ourWindowSetting))
            {
                win.Run();
            }
        }
    }
}
