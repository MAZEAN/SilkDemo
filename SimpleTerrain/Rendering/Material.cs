namespace SimpleTerrain.Rendering;
using System.Numerics;

public class Material
{
    public GLShader Shader { get; set; }
    public GLTexture? Texture { get; set; }
    public Vector4 Color { get; set; } = Vector4.One;
    public Vector2 UvScale { get; set; } = Vector2.One;
    public Vector2 UvOffset { get; set; } = Vector2.Zero;

    public Material(GLShader shader, GLTexture? texture = null)
    {
        Shader = shader;
        Texture = texture;
    }

    public void Dispose()
    {
        Shader.Dispose();
        Texture?.Dispose();
    }
}