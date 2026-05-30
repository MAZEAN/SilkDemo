namespace SimpleTerrain.Rendering;

using Silk.NET.OpenGL;
using Config;
using Scene;
using System.Numerics;
using Lighting;

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
        var view       = _camera.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix();

        foreach (var (shader, entities) in scene.GetEntitiesByShader())
        {
            shader.Use();
            shader.SetUniform("uView",        view);
            shader.SetUniform("uProjection",  projection);
            shader.SetUniform("uCameraPos",   _camera.GetPosition());
            UploadLights(shader, scene.Lighting); 

            foreach (var entity in entities)
            {
                entity.Material.Texture?.Bind();
                shader.SetUniform("uTexture0", 0);
                shader.SetUniform("uModel", entity.Transform.WorldMatrix);
                shader.SetUniform("uUvScale",  entity.Material.UvScale);
                shader.SetUniform("uUvOffset", entity.Material.UvOffset);

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
    
    private void UploadLights(GLShader shader, LightingSystem lights)
    {
        // directional
        var dir = lights.DirectionalLights.FirstOrDefault(l => l.Enabled);
        if (dir != null)
        {
            shader.SetUniform("uDirLight.direction", dir.Direction);
            shader.SetUniform("uDirLight.color",     dir.Color);
            shader.SetUniform("uDirLight.intensity",  dir.Intensity);
        }

        // point lights
        var points = lights.PointLights.Where(l => l.Enabled).ToList();
        shader.SetUniform("uPointLightCount", points.Count);
        
        for (int i = 0; i < points.Count; i++)
        {
            shader.SetUniform($"uPointLights[{i}].position",  points[i].Position);
            shader.SetUniform($"uPointLights[{i}].color",     points[i].Color);
            shader.SetUniform($"uPointLights[{i}].intensity", points[i].Intensity);
            shader.SetUniform($"uPointLights[{i}].constant",  points[i].Constant);
            shader.SetUniform($"uPointLights[{i}].linear",    points[i].Linear);
            shader.SetUniform($"uPointLights[{i}].quadratic", points[i].Quadratic);
        }
    }
}