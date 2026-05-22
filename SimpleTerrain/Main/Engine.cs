namespace SimpleTerrain.Main;

using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;

using Config;
using Scene;
using Rendering;

public class Engine
{
    private IWindow _window = null!;
    private GL _gl = null!;

    private Camera _camera = null!;
    private AppConfig _config = null!;

    private Renderer _renderer = null!;
    private InputSystem _input = null!;

    private Model _model = null!;
    private GLShader _shader = null!;
    private GLTexture _texture = null!;

    public void Run()
    {
        _config = new AppConfig();

        var options = CreateWindowOptions();

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnResize;
        _window.Closing += OnClose;

        _window.Run();
        _window.Dispose();
    }

    private WindowOptions CreateWindowOptions()
    {
        var options = WindowOptions.Default;

        options.Size = new Vector2D<int>(
            _config.Window.Width,
            _config.Window.Height
        );

        options.Title = _config.Window.Title;
        options.VSync = _config.Window.EnableVSync;
        options.Samples = _config.Window.Samples;

        return options;
    }

    private void OnLoad()
    {
        InitializeOpenGl();

        var size = _window.FramebufferSize;

        _camera = new Camera(
            _config.Camera,
            new Vector3(0f, 0f, 3f),
            new Vector3(0f, 0f, -1f),
            Vector3.UnitY,
            -90f,
            0f
        );

        _camera.SetAspectRatio(size);

        _renderer = new Renderer(_gl, _camera, _config);
        _input = new InputSystem(_window, _camera, _config);
        
        _input.Initialize();
        
        _shader = new GLShader(_gl, "Assets/Shaders/shader.vert", "Assets/Shaders/shader.frag");
        _texture = new GLTexture(_gl, "Assets/Textures/wall.jpg");
        _model = new Model(_gl, "Assets/Models/cube.model");
    }
    
    private void InitializeOpenGl()
    {
        _gl = GL.GetApi(_window);
        
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(GLEnum.Multisample);
        _gl.Viewport(_window.FramebufferSize);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private void OnUpdate(double deltaTime)
    {
        float dt = (float) deltaTime;

        _input.UpdateMovement(dt);
    }

    private void OnRender(double deltaTime)
    {
        _renderer.Render(_model, _shader, _texture, (float) deltaTime);
    }

    private void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _camera.SetAspectRatio(size);
    }

    private void OnClose()
    {
        _model?.Dispose();
        _shader?.Dispose();
        _texture?.Dispose();
    }
}