namespace SimpleTerrain.Scene;
using System.Text.Json.Serialization;

public class SceneDefinition
{
    [JsonPropertyName("entities")]
    public List<EntityDefinition> Entities { get; set; } = [];
}

public class EntityDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    // either "material" OR inline "shader"/"texture", not both
    [JsonPropertyName("material")]
    public string? Material { get; set; }

    [JsonPropertyName("shader")]
    public string? Shader { get; set; }

    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("position")]
    public float[] Position { get; set; } = [0f, 0f, 0f];

    [JsonPropertyName("scale")]
    public float[] Scale { get; set; } = [1f, 1f, 1f];

    [JsonPropertyName("rotation")]
    public float[]? Rotation { get; set; }
}

public class MaterialDefinition
{
    [JsonPropertyName("shader")]
    public string Shader { get; set; } = "";

    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("color")]
    public float[] Color { get; set; } = [1f, 1f, 1f, 1f];

    [JsonPropertyName("uvScale")]
    public float[] UvScale { get; set; } = [1f, 1f];

    [JsonPropertyName("uvOffset")]
    public float[] UvOffset { get; set; } = [0f, 0f];
}