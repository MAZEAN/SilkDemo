namespace SimpleTerrain.Rendering;
using Silk.NET.OpenGL;
using Config;
using SimpleTerrain.Scene;

public class Renderer
{
    private readonly GL _gl;
    private readonly Camera _camera;
    private readonly AppConfig _config;

    public Renderer(GL gl, Camera camera, AppConfig config)
    {
        _gl = gl;
        _camera = camera;
        _config = config;
    }

    public void Render(Scene scene, float deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var view       = _camera.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix();

        foreach (var (shader, entities) in scene.GetEntitiesByShader())
        {
            shader.Use();
            shader.SetUniform("uView",       view);
            shader.SetUniform("uProjection", projection);

            foreach (var entity in entities)
            {
                entity.Material.Texture?.Bind();
                shader.SetUniform("uModel", entity.Transform.WorldMatrix);

                foreach (var mesh in entity.Model.Meshes)
                {
                    mesh.Bind();
                    unsafe
                    {
                        _gl.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
                    }
                }
            }
        }
    }
}