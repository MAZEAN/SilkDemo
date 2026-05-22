namespace SimpleTerrain.Config;

public class AppConfig
{
    public WindowConfig Window { get; init; } = new();
    public CameraConfig Camera { get; init; } = new();
    public float MoveSpeed { get; init; } = 2.5f;
}

public class CameraConfig
{
    public float FOV { get; init; } = 45f;
    public float Near { get; init; } = 0.1f;
    public float Far { get; init; } = 100f;
    public float SensitivityX { get; init; } = 0.1f;
    public float SensitivityY { get; init; } = 0.1f;
    public float ZoomSensitivity { get; init; } = 1.5f;
}

public class WindowConfig
{
    public int Width { get; init; } = 800;
    public int Height { get; init; } = 600;
    public string Title { get; init; } = "SimpleTerrain";
    public bool EnableVSync { get; init; } = false;
    public int Samples { get; init; } = 4;
}