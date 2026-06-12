namespace Centauri.Input;

using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Linq;
using System.Numerics;

using Config;
using World;
using Rendering.Systems;

public class InputSystem : IDisposable
{
    // Fly  : cursor captured, look + WASD + MMB pan + scroll (the flythrough mode)
    // Edit : cursor free, ImGui interactive, left-click selects (orbit lands in Stage 2)
    private enum Mode { Fly, Edit }

    private const Key ModeToggleKey = Key.Tab; // Fly <-> Edit

    private readonly IWindow _window;
    private readonly Scene _scene;
    private readonly AppConfig _config;
    private readonly RenderingSystem _renderingSystem;

    private IKeyboard _keyboard = null!;
    public IInputContext InputContext { get; private set; } = null!;

    private Mode _mode = Mode.Fly;
    private bool _pan;
    private Vector2 _mousePos;

    private readonly Dictionary<Camera, CameraController> _controllers = new();

    public InputSystem(IWindow window, Scene scene, AppConfig config, RenderingSystem renderingSystem)
    {
        _window          = window;
        _scene           = scene;
        _config          = config;
        _renderingSystem = renderingSystem;

        Initialize();
    }

    public void Initialize()
    {
        InputContext = _window.CreateInput();
        _keyboard = InputContext.Keyboards.FirstOrDefault()
                    ?? throw new InvalidOperationException("Keyboard not available");

        _keyboard.KeyDown += OnKeyDown;

        foreach (var mouse in InputContext.Mice)
        {
            mouse.Cursor.CursorMode = CursorMode.Raw; // start in Fly mode

            mouse.MouseMove += OnMouseMove;
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp   += OnMouseUp;
            mouse.Scroll    += OnMouseWheel;
        }
    }

    public void Update(float deltaTime)
    {
        if (_mode != Mode.Fly) return;
        GetController(_scene.GetActiveCamera()).UpdateMovement(_keyboard, deltaTime);
    }

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        _mousePos = position;

        if (_mode != Mode.Fly) return; // Edit-mode camera nav comes in Stage 2

        var controller = GetController(_scene.GetActiveCamera());
        if (_pan) controller.Pan(position);
        else      controller.Look(position);
    }

    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        switch (_mode)
        {
            case Mode.Fly when button == MouseButton.Middle:
                _pan = true;
                GetController(_scene.GetActiveCamera()).BeginDrag();
                break;

            case Mode.Edit when button == MouseButton.Left && !_renderingSystem.ImGuiWantsMouse:
                PickAtCursor();
                break;
        }
    }

    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        if (button == MouseButton.Middle && _pan)
        {
            _pan = false;
            GetController(_scene.GetActiveCamera()).BeginDrag();
        }
    }

    private void OnMouseWheel(IMouse mouse, ScrollWheel scroll)
    {
        if (_mode != Mode.Fly) return; // Edit-mode dolly comes in Stage 2
        GetController(_scene.GetActiveCamera()).Zoom(scroll);
    }

    private void PickAtCursor()
    {
        var cam = _scene.GetActiveCamera();
        var ray = cam.ScreenPointToRay(_mousePos, new Vector2(_window.Size.X, _window.Size.Y));
        _scene.Selected = _scene.Pick(ray);
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int code)
    {
        if (key == Key.Escape)    { _window.Close(); return; }
        if (key == ModeToggleKey) { ToggleMode();    return; }

        if (_renderingSystem.ImGuiWantsKeyboard) return;

        switch (key)
        {
            case Key.M:  _renderingSystem.ToggleStatsOverlay();          break;
            case Key.C:  _scene.CycleCamera(); ResetActiveController();  break; // moved off Tab

            case Key.F1: _config.Debug.ToggleShowDebugView();     break;
            case Key.F2: _config.Debug.ToggleShowBoundingBoxes(); break;
            case Key.F3: _config.Debug.ToggleShowFrustums();      break;
            case Key.F4: _config.Debug.ToggleShowCameras();       break;
            case Key.F5: _config.Debug.ToggleEnableCulling();     break;
            case Key.F6: _config.Debug.ToggleShowGrid();          break;
        }
    }

    private void ToggleMode()
    {
        _mode = _mode == Mode.Fly ? Mode.Edit : Mode.Fly;
        _pan = false;
        SetCursor(_mode == Mode.Fly ? CursorMode.Raw : CursorMode.Normal);
        if (_mode == Mode.Fly) ResetActiveController();
    }

    private void SetCursor(CursorMode mode)
    {
        foreach (var mouse in InputContext.Mice)
            mouse.Cursor.CursorMode = mode;
    }

    private CameraController GetController(Camera cam)
    {
        if (!_controllers.TryGetValue(cam, out var controller))
        {
            controller = new CameraController(cam, _config.Camera);
            _controllers[cam] = controller;
        }
        return controller;
    }

    private void ResetActiveController()
        => GetController(_scene.GetActiveCamera()).BeginDrag();

    public void Dispose()
    {
        _keyboard.KeyDown -= OnKeyDown;
        foreach (var mouse in InputContext.Mice)
        {
            mouse.MouseMove -= OnMouseMove;
            mouse.MouseDown -= OnMouseDown;
            mouse.MouseUp   -= OnMouseUp;
            mouse.Scroll    -= OnMouseWheel;
        }
        InputContext.Dispose();
    }
}