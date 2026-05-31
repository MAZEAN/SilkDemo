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
    
    private bool _projectionDirty = true;

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

            // always upload — camera moves every frame
            shader.SetUniform("uView",      view);
            shader.SetUniform("uCameraPos", _camera.GetPosition());

            // only on first frame or window resize
            if (_projectionDirty)
            {
                shader.SetUniform("uProjection", projection);
                shader.SetUniform("uAlbedoMap",    0);
                shader.SetUniform("uNormalMap",    1);
                shader.SetUniform("uRoughnessMap", 2);
                shader.SetUniform("uMetallicMap",  3);
                shader.SetUniform("uAOMap",        4);
            }
            UploadLights(shader, scene.Lighting);

            foreach (var entity in entities)
            {
                var mat = entity.Material;

                // ALWAYS rebind textures — OpenGL slots are global state
                // previous entity may have bound different textures to these slots
                mat.Albedo?.Bind(TextureUnit.Texture0);
                mat.Normal?.Bind(TextureUnit.Texture1);
                mat.Roughness?.Bind(TextureUnit.Texture2);
                mat.Metallic?.Bind(TextureUnit.Texture3);
                mat.AO?.Bind(TextureUnit.Texture4);

                // ALWAYS upload has-flags — they differ per entity
                shader.SetUniform("uHasAlbedo",    mat.Albedo    != null ? 1 : 0);
                shader.SetUniform("uHasNormal",    mat.Normal    != null ? 1 : 0);
                shader.SetUniform("uHasRoughness", mat.Roughness != null ? 1 : 0);
                shader.SetUniform("uHasMetallic",  mat.Metallic  != null ? 1 : 0);
                shader.SetUniform("uHasAO",        mat.AO        != null ? 1 : 0);

                // ALWAYS upload transform — entity may have moved
                shader.SetUniform("uModel", entity.Transform.WorldMatrix);
                if (Matrix4x4.Invert(entity.Transform.WorldMatrix, out var invModel))
                    shader.SetUniformMat3x3("uNormalMatrix", Matrix4x4.Transpose(invModel));
                else
                    shader.SetUniformMat3x3("uNormalMatrix", Matrix4x4.Transpose(entity.Transform.WorldMatrix));
                
                shader.SetUniform("uRoughnessValue", mat.RoughnessValue);
                shader.SetUniform("uMetallicValue",  mat.MetallicValue);
                shader.SetUniform("uColor",          mat.Color);
                shader.SetUniform("uUvScale",        mat.UvScale);
                shader.SetUniform("uUvOffset",       mat.UvOffset);

                foreach (var mesh in entity.Model.Meshes)
                {
                    mesh.Bind();
                    unsafe
                    {
                        _gl.DrawElements(PrimitiveType.Triangles, mesh.IndexCount,
                            DrawElementsType.UnsignedInt, (void*)0);
                    }
                }
            }
        }
        
        if (_projectionDirty)      _projectionDirty = false;
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
        
        // spotlights
        var spots = lights.SpotLights.Where(l => l.Enabled).ToList();
        shader.SetUniform("uSpotLightCount", spots.Count);
        for (int i = 0; i < spots.Count; i++)
        {
            shader.SetUniform($"uSpotLights[{i}].position",    spots[i].Position);
            shader.SetUniform($"uSpotLights[{i}].direction",   spots[i].Direction);
            shader.SetUniform($"uSpotLights[{i}].color",       spots[i].Color);
            shader.SetUniform($"uSpotLights[{i}].intensity",   spots[i].Intensity);
            shader.SetUniform($"uSpotLights[{i}].innerCutoff", MathF.Cos(spots[i].InnerCutoff * MathF.PI / 180f));
            shader.SetUniform($"uSpotLights[{i}].outerCutoff", MathF.Cos(spots[i].OuterCutoff * MathF.PI / 180f));
        }
    }
    
    public void OnResize() => _projectionDirty = true;
}