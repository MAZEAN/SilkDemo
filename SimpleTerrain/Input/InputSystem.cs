namespace SimpleTerrain.Input;

using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Linq;
using System.Numerics;

using Config;
using Scene;

public class InputSystem
{
    
    private readonly IWindow _window;
    private readonly Camera _camera;
    private readonly AppConfig _config;
    
    private CameraController _cameraController = null!;
    private IKeyboard _keyboard= null!;
    public InputSystem(IWindow window, Camera camera, AppConfig config)
    {
        _window = window;
        _camera = camera;
        _config = config;
    }

    public void Initialize()
    {
        var input = _window.CreateInput();
        
        _cameraController = new CameraController(_camera, _config.Camera);
        _keyboard = input.Keyboards.FirstOrDefault()
                    ?? throw new InvalidOperationException("Keyboard not available");
        _keyboard.KeyDown += OnKeyDown;
        
        foreach (var mouse in input.Mice)
        {
            mouse.Cursor.CursorMode = CursorMode.Raw;
            mouse.MouseMove += _cameraController.OnMouseMove;
            mouse.Scroll += _cameraController.OnMouseWheel;
        }
    }
    
    public void UpdateMovement(float deltaTime)
    {
        var moveSpeed = _config.MoveSpeed * deltaTime;

        if (_keyboard.IsKeyPressed(Key.W))
        {
            //Move forwards
            _camera.UpdatePosition(_camera.Forward * moveSpeed);
        }
        if (_keyboard.IsKeyPressed(Key.S))
        {
            //Move backwards
            _camera.UpdatePosition(_camera.Forward * -moveSpeed);
        }
        if (_keyboard.IsKeyPressed(Key.A))
        {
            //Move left
            _camera.UpdatePosition(Vector3.Normalize(Vector3.Cross(_camera.Forward, _camera.Up)) * -moveSpeed);
        }
        if (_keyboard.IsKeyPressed(Key.D))
        {
            //Move right
            _camera.UpdatePosition(Vector3.Normalize(Vector3.Cross(_camera.Forward, _camera.Up)) * moveSpeed);
        }
        if (_keyboard.IsKeyPressed(Key.Space))
        {
            //Move up
            _camera.UpdatePosition(_camera.Up * moveSpeed);
        }
        if (_keyboard.IsKeyPressed(Key.ControlLeft))
        {
            //Move down
            _camera.UpdatePosition(_camera.Up * -moveSpeed);
        }
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int code)
    {
        if (key == Key.Escape)
        {
            _window.Close();
        }
    }
}