using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;

namespace GrafkomUAS
{
    class Program
    {
        static void Main(string[] args)
        {
            var ourWindowSetting = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "UAS GRAFIKA KOMPUTER"
            };
            using (var win = new Windows(GameWindowSettings.Default, ourWindowSetting))
            {
                win.Run();
            }
        }
    }
}
