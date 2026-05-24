namespace SimpleTerrain.Main;

using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;
using Config;
using Scene;
using Rendering;
using Input;

public class Engine
{
    private IWindow _window = null!;
    private GL _gl = null!;
    private Camera _camera = null!;
    private AppConfig _config = null!;
    private Renderer _renderer = null!;
    private InputSystem _input = null!;
    private Scene _scene = null!;
    private GridRenderer _grid = null!;

    public void Run()
    {
        _config = new AppConfig();
        _scene  = new Scene();

        var options = CreateWindowOptions();
        _window = Window.Create(options);

        _window.Load              += OnLoad;
        _window.Update            += OnUpdate;
        _window.Render            += OnRender;
        _window.FramebufferResize += OnResize;
        _window.Closing           += OnClose;

        _window.Run();
        _window.Dispose();
    }

    private WindowOptions CreateWindowOptions()
    {
        var options = WindowOptions.Default;
        options.Size    = new Vector2D<int>(_config.Window.Width, _config.Window.Height);
        options.Title   = _config.Window.Title;
        options.VSync   = _config.Window.EnableVSync;
        options.Samples = _config.Window.Samples;
        return options;
    }

    private void OnLoad()
    {
        try
        {
            InitializeOpenGL();
            _camera = new Camera(
                _config.Camera,
                new Vector3(0f, 0f, 3f),
                new Vector3(0f, 0f, -1f),
                Vector3.UnitY,
                -90f,
                0f
            );
            _camera.SetAspectRatio(_window.FramebufferSize);

            _renderer = new Renderer(_gl, _camera, _config);
            _input    = new InputSystem(_window, _camera, _config);
            _input.Initialize();

            SceneLoader.Load("Assets/scene.json", _scene, _gl);
            _grid = new GridRenderer(_gl);
        }
        catch (Exception e)
        {
            Console.WriteLine($"OnLoad failed: {e}");
            throw;
        }
    }

    private void InitializeOpenGL()
    {
        _gl = GL.GetApi(_window);
        _gl.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(GLEnum.Multisample);
        _gl.Viewport(_window.FramebufferSize);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private void OnUpdate(double deltaTime) => _input.UpdateMovement((float) deltaTime);

    private void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        _renderer.Render(_scene, (float) deltaTime);
        _grid.Render(_camera);
    }

    private void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _camera.SetAspectRatio(size);
    }
    
    private void OnClose()
    {
        _scene.Dispose();
        //_grid.Dispose();
    }
}