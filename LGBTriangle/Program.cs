using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace LGBTriangle
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var windowSettings = new NativeWindowSettings
            {
                Size = new Vector2i(800, 600),
                Title = "Hello world!"
            };

            using var window = new Window(GameWindowSettings.Default, windowSettings);
            window.Run();
        }
    }
}