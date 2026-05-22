namespace SimpleTerrain.Input;

using Silk.NET.Input;
using System.Numerics;
using Scene;
using Config;

public class CameraController
{
    private Vector2 _lastMousePosition;
    private readonly Camera _camera;
    private readonly CameraConfig _config;
    
    public CameraController(Camera camera, CameraConfig config)
    {
        _camera = camera;
        _config = config;
    }

    public void OnMouseMove(IMouse mouse, Vector2 position)
    {
        if (_lastMousePosition == default)
        {
            _lastMousePosition = position;
            return;
        }

        var xOffset = (position.X - _lastMousePosition.X) * _config.SensitivityX;
        var yOffset = (position.Y - _lastMousePosition.Y) * _config.SensitivityY;

        _lastMousePosition = position;

        _camera.ModifyDirection(xOffset, yOffset);
    }
}