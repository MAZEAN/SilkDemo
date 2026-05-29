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
    
    private int _frameCount;
    private double _fpsTimer;

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
        var monitor = Monitor.GetMonitors(null)
            .OrderByDescending(m =>
            {
                var r = m.VideoMode.Resolution;
                return r.HasValue ? r.Value.X * r.Value.Y : 0;
            })
            .First();

        options.WindowState = WindowState.Maximized;
        options.Position = monitor.Bounds.Center;
        options.Size = monitor.Bounds.Size;
        options.Title = _config.Window.Title;
        options.VSync = _config.Window.EnableVSync;
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
            _grid = new GridRenderer(_gl, _config.Window);
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

        // background color when clearing the frame
        var color = _config.Window.ClearColor;
        _gl.ClearColor(color.X, color.Y, color.Z, color.W);

        // discard fragments that are behind already-drawn geometry
        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthFunc(DepthFunction.Less);

        // smooth jagged edges — sample count set in WindowConfig.Samples
        _gl.Enable(GLEnum.Multisample);

        // define the render region — important for high-DPI displays
        _gl.Viewport(_window.FramebufferSize);

        // enable alpha transparency
        _gl.Enable(EnableCap.Blend);

        // blend color and alpha channels separately:
        // color: srcAlpha * src + (1 - srcAlpha) * dst  — standard transparency
        // alpha: 1 * src + 0 * dst                      — preserve source alpha
        _gl.BlendFuncSeparate(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha,
            BlendingFactor.One,
            BlendingFactor.Zero
        );

        // skip rendering triangles facing away from camera — halves fragment work
        // requires consistent counter-clockwise winding in exported meshes (Blender default)
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(TriangleFace.Back);
        _gl.FrontFace(FrontFaceDirection.Ccw);

        // needed if you add skybox or reflections later — removes seams on cubemap edges
        _gl.Enable(EnableCap.TextureCubeMapSeamless);

        // default fill mode — change to PolygonMode.Line for wireframe debugging
        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
    }

    private void OnUpdate(double deltaTime)
    {
        _input.UpdateMovement((float)deltaTime);

        _fpsTimer   += deltaTime;
        _frameCount += 1;

        if (_fpsTimer >= 1.0)
        {
            var fps = _frameCount / _fpsTimer;
            var ms  = 1000.0 / fps;

            _window.Title = $"{_config.Window.Title} — {fps:F1} FPS ({ms:F2} ms)";

            _frameCount = 0;
            _fpsTimer   = 0;
        }
    }

    private void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        _grid.Render(_camera);
        _renderer.Render(_scene, (float) deltaTime);
    }

    private void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _camera.SetAspectRatio(size);
    }
    
    private void OnClose()
    {
        _scene.Dispose();
        _grid.Dispose();
    }
}