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
    
    private readonly List<PointLight> _activePointLights = new();
    private readonly List<SpotLight> _activeSpotLights  = new();
    
    private bool _projectionDirty = true;

    public Renderer(GL gl, Camera camera, AppConfig config)
    {
        _gl = gl;
        _camera = camera;
        _config = config;
    }

    public void Render(Scene scene, float deltaTime)
    {
        var view = _camera.GetViewMatrix();
        var cameraPosition = _camera.GetPosition();
        bool lightingDirty = scene.Lighting.IsDirty;

        foreach (var (shader, entities) in scene.GetEntitiesByShader())
        {
            shader.Use();
            
            shader.SetUniform("uView",      view);
            shader.SetUniform("uCameraPos", cameraPosition);
            
            if (_projectionDirty) UploadGlobalUniforms(shader);
            if (lightingDirty) UploadLighting(shader, scene.Lighting);

            foreach (var entity in entities)
            {
                var mat = entity.Material;
                
                BindMaterialTextures(mat);
                UploadMaterialFlags(shader, mat);
                UploadTransform(shader, entity);
                UploadMaterialProperties(shader, mat);

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
        
        if (_projectionDirty) _projectionDirty = false;
        if (lightingDirty) scene.Lighting.ClearDirty();
    }

    private void BindMaterialTextures(Material mat)
    {
        mat.Albedo?.Bind(TextureUnit.Texture0);
        mat.Normal?.Bind(TextureUnit.Texture1);
        mat.Roughness?.Bind(TextureUnit.Texture2);
        mat.Metallic?.Bind(TextureUnit.Texture3);
        mat.AO?.Bind(TextureUnit.Texture4);
    }

    private void UploadGlobalUniforms(GLShader shader)
    {
        var projection = _camera.GetProjectionMatrix();
        shader.SetUniform("uProjection", projection);
        shader.SetUniform("uAlbedoMap",    0);
        shader.SetUniform("uNormalMap",    1);
        shader.SetUniform("uRoughnessMap", 2);
        shader.SetUniform("uMetallicMap",  3);
        shader.SetUniform("uAOMap",        4);
    }

    private void UploadMaterialFlags(GLShader shader, Material mat)
    {
        shader.SetUniform("uHasAlbedo",    mat.Albedo    != null ? 1 : 0);
        shader.SetUniform("uHasNormal",    mat.Normal    != null ? 1 : 0);
        shader.SetUniform("uHasRoughness", mat.Roughness != null ? 1 : 0);
        shader.SetUniform("uHasMetallic",  mat.Metallic  != null ? 1 : 0);
        shader.SetUniform("uHasAO",        mat.AO        != null ? 1 : 0);
    }

    private void UploadTransform(GLShader shader, Entity entity)
    {
        shader.SetUniform("uModel", entity.Transform.WorldMatrix);
        if (Matrix4x4.Invert(entity.Transform.WorldMatrix, out var invModel))
            shader.SetUniformMat3x3("uNormalMatrix", Matrix4x4.Transpose(invModel));
        else
            shader.SetUniformMat3x3("uNormalMatrix", Matrix4x4.Transpose(entity.Transform.WorldMatrix));
    }

    private void UploadMaterialProperties(GLShader shader, Material mat)
    {
        shader.SetUniform("uRoughnessValue", mat.RoughnessValue);
        shader.SetUniform("uMetallicValue",  mat.MetallicValue);
        shader.SetUniform("uColor",          mat.Color);
        shader.SetUniform("uUvScale",        mat.UvScale);
        shader.SetUniform("uUvOffset",       mat.UvOffset);
    }
    
    private void UploadLighting(GLShader shader, LightingSystem lights)
    {
        // directional
        var dir = lights.DirectionalLights.FirstOrDefault(l => l.Enabled);
        if (dir != null)
        {
            shader.SetUniform("uDirLight.direction", dir.Direction);
            shader.SetUniform("uDirLight.color",     dir.Color);
            shader.SetUniform("uDirLight.intensity",  dir.Intensity);
        }
        UploadPointLights(shader, lights);
        UploadSpotLights(shader, lights);
    }

    private void UploadPointLights(GLShader shader, LightingSystem lights)
    {
        _activePointLights.Clear();
        foreach (var l in lights.PointLights)
        {
            if (l.Enabled)
                _activePointLights.Add(l);
        }

        int pointLightsCount = _activePointLights.Count;
        shader.SetUniform("uPointLightCount", pointLightsCount);
        
        for (int i = 0; i < pointLightsCount; i++)
        {
            var light = _activePointLights[i];
            shader.SetUniform($"uPointLights[{i}].position",  light.Position);
            shader.SetUniform($"uPointLights[{i}].color",     light.Color);
            shader.SetUniform($"uPointLights[{i}].intensity", light.Intensity);
            shader.SetUniform($"uPointLights[{i}].constant",  light.Constant);
            shader.SetUniform($"uPointLights[{i}].linear",    light.Linear);
            shader.SetUniform($"uPointLights[{i}].quadratic", light.Quadratic);
        }
    }

    private void UploadSpotLights(GLShader shader, LightingSystem lights)
    {
        _activeSpotLights.Clear();
        foreach (var l in lights.SpotLights)
        {
            if (l.Enabled)
                _activeSpotLights.Add(l);
        }
        
        int spotLightsCount = _activeSpotLights.Count;
        shader.SetUniform("uSpotLightCount", spotLightsCount);
        for (int i = 0; i < spotLightsCount; i++)
        {
            var light = _activeSpotLights[i];
            shader.SetUniform($"uSpotLights[{i}].position",    light.Position);
            shader.SetUniform($"uSpotLights[{i}].direction",   light.Direction);
            shader.SetUniform($"uSpotLights[{i}].color",       light.Color);
            shader.SetUniform($"uSpotLights[{i}].intensity",   light.Intensity);
            shader.SetUniform($"uSpotLights[{i}].innerCutoff", MathF.Cos(light.InnerCutoff * MathF.PI / 180f));
            shader.SetUniform($"uSpotLights[{i}].outerCutoff", MathF.Cos(light.OuterCutoff * MathF.PI / 180f));
        }
    }
    
    public void OnResize() => _projectionDirty = true;
}