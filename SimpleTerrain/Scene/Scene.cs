namespace SimpleTerrain.Scene;

using Rendering;
using Lighting;

public class Scene
{
    private readonly List<Entity> _entities = new();
    public IReadOnlyList<Entity> Entities => _entities;
    public LightingSystem Lighting { get; } = new();

    private Dictionary<GLShader, List<Entity>> _shaderGroups = new();
    
    private bool _shaderGroupsDirty = true;

    public Dictionary<GLShader, List<Entity>> GetEntitiesByShader()
    {
        if (!_shaderGroupsDirty) return _shaderGroups;

        _shaderGroups = _entities
            .GroupBy(e => e.Material.Shader)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(e => e.Material.SortKey).ToList()
            );
        
        ClearDirty();
        return _shaderGroups;
    }

    public void AddEntity(Entity entity)
    {
        _entities.Add(entity);
        _shaderGroupsDirty = true;
    }

    public void RemoveEntity(Entity entity)
    {
        _entities.Remove(entity);
        _shaderGroupsDirty = true;
    }

    public void ClearDirty()
    {
        _shaderGroupsDirty = false;
    }
    
    public void MarkDirty()
    {
        _shaderGroupsDirty = true;
    }
    
    public void Dispose()
    {
        foreach (var entity in _entities)
        {
            entity.Dispose();
        }
        _entities.Clear();
    }
}