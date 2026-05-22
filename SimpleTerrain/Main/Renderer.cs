namespace SimpleTerrain.Main;

using Silk.NET.OpenGL;
using System.Numerics;

using Config;
using Scene;
using Rendering;

public class Renderer
{
    private GL _gl;
    private Camera _camera;
    private AppConfig _config;

    private float _passedTime = 0f;

    public Renderer(GL gl, Camera camera, AppConfig config)
    {
        _gl = gl;
        _camera = camera;
        _config = config;
    }

    public void Render(Model model, GLShader shader, GLTexture texture, float deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        texture.Bind();
        shader.Use();
        shader.SetUniform("uTexture0", 0);

        _passedTime += deltaTime;

        //Use elapsed time to convert to radians to allow our cube to rotate over time
        var difference = _passedTime * 100;
        
        var modelMatrix = Matrix4x4.CreateRotationY(Core.MathHelper.DegreesToRadians(difference)) * Matrix4x4.CreateRotationX(Core.MathHelper.DegreesToRadians(difference));
        var view = _camera.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix();

        foreach (var mesh in model.Meshes)
        {
            mesh.Bind();
            shader.SetUniform("uModel", modelMatrix);
            shader.SetUniform("uView", view);
            shader.SetUniform("uProjection", projection);

            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint) mesh.Vertices.Length);
        }
    }
}