namespace SimpleTerrain.Rendering;
using Silk.NET.OpenGL;
using SimpleTerrain.Scene;
using System.Numerics;

public class GridRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly GLShader _shader;
    private readonly Mesh _mesh;

    public GridRenderer(GL gl)
    {
        _gl    = gl;
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
        _shader.Use();
        _shader.SetUniform("uView",       camera.GetViewMatrix());
        _shader.SetUniform("uProjection", camera.GetProjectionMatrix());
        _shader.SetUniform("uCameraPos",   camera.GetPosition()); // add this

        _mesh.Bind();
        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        }
    }

    public void Dispose()
    {
        _mesh.Dispose();
        _shader.Dispose();
    }
}