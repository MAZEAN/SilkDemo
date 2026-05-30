namespace SimpleTerrain.Scene;

using Rendering;
using Lighting;

public class Scene
{
    private readonly List<Entity> _entities = new();
    public IReadOnlyList<Entity> Entities => _entities;
    public LightingSystem Lighting { get; } = new();

    public void AddEntity(Entity entity) => _entities.Add(entity);
    public void RemoveEntity(Entity entity) => _entities.Remove(entity);

    public Dictionary<GLShader, List<Entity>> GetEntitiesByShader()
        => _entities
            .GroupBy(e => e.Material.Shader)
            .ToDictionary(g => g.Key, g => g.ToList());
    
    public void Dispose()
    {
        foreach (var entity in _entities)
        {
            entity.Dispose();
        }
        _entities.Clear();
    }
}