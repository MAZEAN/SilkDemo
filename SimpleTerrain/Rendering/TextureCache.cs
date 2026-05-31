namespace SimpleTerrain.Rendering;

using Silk.NET.OpenGL;
using Utils;

// TextureCache.cs
public class TextureCache : IDisposable
{
    private readonly LRUCache<string, GLTexture> _cache;
    private readonly List<GLTexture> _all = new();
    private readonly GL _gl;

    public TextureCache(GL gl, int capacity)
    {
        _gl    = gl;
        _cache = new LRUCache<string, GLTexture>(capacity);
    }

    public GLTexture Get(string path)
    {
        var cached = _cache.Get(path);
        if (cached != null) return cached;

        var texture = new GLTexture(_gl, path);
        _cache.Put(path, texture);
        _all.Add(texture);
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in _all)
            texture.Dispose();
        _all.Clear();
    }
}