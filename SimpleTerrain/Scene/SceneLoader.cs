namespace SimpleTerrain.Scene;
using System.Text.Json;
using System.Numerics;
using SimpleTerrain.Rendering;
using Silk.NET.OpenGL;

public static class SceneLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void Load(string path, Scene scene, GL gl)
    {
        var json = File.ReadAllText(path);
        var def  = JsonSerializer.Deserialize<SceneDefinition>(json, Options)
                   ?? throw new Exception($"Failed to deserialize scene file: {path}");

        foreach (var e in def.Entities)
        {
            var model    = new Model(gl, e.Model);
            var material = e.Material != null
                ? LoadMaterialFile(gl, e.Material)
                : LoadInlineMaterial(gl, e);

            var entity = new Entity(model, material);

            entity.Transform.Position = new Vector3(e.Position[0], e.Position[1], e.Position[2]);
            entity.Transform.Scale    = new Vector3(e.Scale[0],    e.Scale[1],    e.Scale[2]);

            if (e.Rotation is { Length: 3 })
                entity.Transform.SetEulerAngles(e.Rotation[0], e.Rotation[1], e.Rotation[2]);

            scene.AddEntity(entity);
        }
    }

    private static Material LoadMaterialFile(GL gl, string path)
    {
        var json = File.ReadAllText(path);
        var def  = JsonSerializer.Deserialize<MaterialDefinition>(json, Options)
                   ?? throw new Exception($"Failed to deserialize material file: {path}");

        var shader  = new GLShader(gl, def.Shader + ".vert", def.Shader + ".frag");
        var texture = def.Texture != null ? new GLTexture(gl, def.Texture) : null;

        return new Material(shader, texture)
        {
            Color    = new Vector4(def.Color[0],    def.Color[1],    def.Color[2],    def.Color[3]),
            UvScale  = new Vector2(def.UvScale[0],  def.UvScale[1]),
            UvOffset = new Vector2(def.UvOffset[0], def.UvOffset[1])
        };
    }

    private static Material LoadInlineMaterial(GL gl, EntityDefinition e)
    {
        if (e.Shader == null)
            throw new Exception($"Entity '{e.Name}' has no material file or inline shader.");

        var shader  = new GLShader(gl, e.Shader + ".vert", e.Shader + ".frag");
        var texture = e.Texture != null ? new GLTexture(gl, e.Texture) : null;

        return new Material(shader, texture);
    }
}