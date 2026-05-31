namespace SimpleTerrain.Config;

using System.Numerics;
using Silk.NET.Windowing;

public class AppConfig
{
    public WindowConfig Window { get; init; } = new();
    public CameraConfig Camera { get; init; } = new();
    public RenderConfig Render { get; init; } = new();
    public float MoveSpeed { get; init; } = 2.5f;
}

public class RenderConfig
{
    public string ScenePath { get; init; } = "Assets/scene.json";
    public int TextureCacheSize { get; init; } = 128;
}

public class CameraConfig
{
    public float FOV { get; init; } = 45f;
    public float Near { get; init; } = 0.1f;
    public float Far { get; init; } = 1000f;
    public float SensitivityX { get; init; } = 0.1f;
    public float SensitivityY { get; init; } = 0.1f;
    public float ZoomSensitivity { get; init; } = 1.5f;
}

public class WindowConfig
{
    
    public string Title { get; init; } = "SimpleTerrain";
    public WindowState WindowState = WindowState.Maximized;
    public IMonitor PrimaryMonitor = FindPrimaryMonitor();
    public bool EnableVSync { get; init; } = true;
    public int Samples { get; init; } = 4;
    public Vector4 ClearColor { get; init; } = new(0.3f, 0.3f, 0.3f, 1.0f);

    private static IMonitor FindPrimaryMonitor()
    {
        var monitor = Monitor.GetMonitors(null)
            .OrderByDescending(m =>
            {
                var r = m.VideoMode.Resolution;
                return r.HasValue ? r.Value.X * r.Value.Y : 0;
            })
            .First();
        return monitor;
    }
}