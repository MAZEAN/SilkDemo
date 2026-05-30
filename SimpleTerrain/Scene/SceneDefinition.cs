namespace SimpleTerrain.Scene;
using System.Text.Json.Serialization;

public class SceneDefinition
{
    [JsonPropertyName("entities")]
    public List<EntityDefinition> Entities { get; set; } = [];

    [JsonPropertyName("lights")]
    public LightsDefinition Lights { get; set; } = new(); 
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

public class LightsDefinition
{
    [JsonPropertyName("directional")]
    public List<DirectionalLightDefinition> Directional { get; set; } = [];

    [JsonPropertyName("point")]
    public List<PointLightDefinition> Point { get; set; } = [];

    [JsonPropertyName("spot")]
    public List<SpotLightDefinition> Spot { get; set; } = [];
}

public class DirectionalLightDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("direction")]
    public float[] Direction { get; set; } = [0f, -1f, 0f];

    [JsonPropertyName("color")]
    public float[] Color { get; set; } = [1f, 1f, 1f];

    [JsonPropertyName("intensity")]
    public float Intensity { get; set; } = 1.0f;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}

public class PointLightDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("position")]
    public float[] Position { get; set; } = [0f, 0f, 0f];

    [JsonPropertyName("color")]
    public float[] Color { get; set; } = [1f, 1f, 1f];

    [JsonPropertyName("intensity")]
    public float Intensity { get; set; } = 1.0f;

    [JsonPropertyName("constant")]
    public float Constant { get; set; } = 1.0f;

    [JsonPropertyName("linear")]
    public float Linear { get; set; } = 0.09f;

    [JsonPropertyName("quadratic")]
    public float Quadratic { get; set; } = 0.032f;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}

public class SpotLightDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("position")]
    public float[] Position { get; set; } = [0f, 0f, 0f];

    [JsonPropertyName("direction")]
    public float[] Direction { get; set; } = [0f, -1f, 0f];

    [JsonPropertyName("color")]
    public float[] Color { get; set; } = [1f, 1f, 1f];

    [JsonPropertyName("intensity")]
    public float Intensity { get; set; } = 1.0f;

    [JsonPropertyName("innerCutoff")]
    public float InnerCutoff { get; set; } = 12.5f;

    [JsonPropertyName("outerCutoff")]
    public float OuterCutoff { get; set; } = 17.5f;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}