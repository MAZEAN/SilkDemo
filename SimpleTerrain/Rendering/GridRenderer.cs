namespace SimpleTerrain.Rendering;
using Silk.NET.OpenGL;
using SimpleTerrain.Scene;
using System.Numerics;

public class GridRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly GLShader _shader;
    private readonly Mesh _mesh;
    private readonly Config.WindowConfig _config;

    public GridRenderer(GL gl, Config.WindowConfig config)
    {
        _gl     = gl;
        _config = config;
        _shader = new GLShader(gl, "Assets/Shaders/grid.vert", "Assets/Shaders/grid.frag");

        // fullscreen quad in NDC space
        float[] vertices =
        [
            -1f,  1f, 0f,   0f, 0f, 1f,   0f, 1f,
            -1f, -1f, 0f,   0f, 0f, 1f,   0f, 0f,
            1f,  1f, 0f,   0f, 0f, 1f,   1f, 1f,
            1f, -1f, 0f,   0f, 0f, 1f,   1f, 0f,
        ];

        uint[] indices = [0, 1, 2, 2, 1, 3];
        _mesh = new Mesh(gl, vertices, indices);
    }

    // GridRenderer.cs
    public void Render(Camera camera)
    {
        SetGL();
        
        _shader.Use();

        _shader.SetUniform("uView", camera.GetViewMatrix());
        _shader.SetUniform("uProjection", camera.GetProjectionMatrix());
        _shader.SetUniform("uCameraPos", camera.GetPosition());
        _shader.SetUniform("background", _config.ClearColor);

        _mesh.Bind();

        unsafe
        {
            _gl.DrawElements(
                PrimitiveType.Triangles,
                6,
                DrawElementsType.UnsignedInt,
                (void*)0
            );
        }

        RestoreGL();
    }

    private void SetGL()
    {
        _gl.DepthFunc(GLEnum.Lequal);
        _gl.DepthMask(false); // Critical: don't let transparent grid overwrite depth
    }

    private void RestoreGL()
    {
        _gl.DepthMask(true);
        _gl.DepthFunc(DepthFunction.Less);
    }

    public void Dispose()
    {
        _mesh.Dispose();
        _shader.Dispose();
    }
}