namespace Centauri.Rendering.Systems;

using Silk.NET.OpenGL;

using Utils.Caching;
using Resources;
using Config;
using Utils.Misc;
using Geometry;

public class ResourceSystem : IDisposable
{
    public AssetCache<GLTexture> Textures { get; }
    public AssetCache<GLShader> Shaders { get; }
    public AssetCache<Model> Models { get; }
    public GLTexture DefaultTexture { get; private set; }
    
    private readonly string _defaultShaderPath;

    public ResourceSystem(GL gl, AppConfig config)
    {
        Textures = new AssetCache<GLTexture>(
            path => new GLTexture(gl, PathResolver.Resolve(path))
        );

        Shaders = new AssetCache<GLShader>(
            shaderBase => new GLShader(gl,
                PathResolver.Resolve(shaderBase + ".vert"),
                PathResolver.Resolve(shaderBase + ".frag")));

        Models = new AssetCache<Model>(
            path => new Model(gl, PathResolver.Resolve(path)));

        DefaultTexture = CreateDefaultTexture(gl);
        
        _defaultShaderPath = config.Render.DefaultShader;
        DefaultTexture = CreateDefaultTexture(gl);
    }
    
    private static GLTexture CreateDefaultTexture(GL gl)
    {
        Span<byte> pixel = [255, 255, 255, 255];
        return new GLTexture(gl, pixel, 1, 1);
    }
    
    public Material CreateDefaultMaterial()
        => new(Shaders.Get(_defaultShaderPath)) { AO = DefaultTexture };

    public void Dispose()
    {
        Textures.Dispose();
        Shaders.Dispose();
        Models.Dispose();
        DefaultTexture.Dispose();

        Console.WriteLine("[ResourceSystem] Disposed all resources");
    }
}