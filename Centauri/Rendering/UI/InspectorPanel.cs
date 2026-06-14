namespace Centauri.Rendering.UI;

using ImGuiNET;
using System.Numerics;

using World;

public class InspectorPanel
{
    private const float Width   = 300f;
    private const float Padding = 10f;
    private const float BgAlpha = 0.85f;

    private static readonly string[] LightTypes = ["None", "Directional", "Point", "Spot"];

    private readonly ImFontPtr _font;

    private Entity? _tracked;
    private Vector3 _euler;   // cached working rotation (deg) for the selected entity
    
    private const ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove          |
                                           ImGuiWindowFlags.NoCollapse      |
                                           ImGuiWindowFlags.NoSavedSettings |
                                           ImGuiWindowFlags.AlwaysAutoResize;

    public InspectorPanel(ImFontPtr font) => _font = font;

    public void Render(Scene scene)
    {
        SetupWindow();

        if (!ImGui.Begin("Inspector", Flags))
        {
            ImGui.End();
            return;
        }

        ImGui.PushFont(_font);

        if (scene.Selected is not { } entity)
        {
            ImGui.TextDisabled("No entity selected");
        }
        else
        {
            if (!ReferenceEquals(entity, _tracked))   // re-seed euler on selection change
            {
                _tracked = entity;
                _euler   = entity.Transform.EulerAngles;
            }

            GUI.Check("Enabled", entity.Enabled, v => entity.Enabled = v);
            ImGui.Spacing();

            DrawTransform(entity);
            DrawMaterial(entity);
            DrawLight(entity);
        }

        ImGui.PopFont();
        ImGui.End();
    }

    private static void SetupWindow()
    {
        var viewport = ImGui.GetMainViewport();
        var anchor = new Vector2(viewport.WorkPos.X + viewport.WorkSize.X - Padding, viewport.WorkPos.Y + Padding);
        
        ImGui.SetNextWindowPos(anchor, ImGuiCond.Always, new Vector2(1f, 0f)); 
        ImGui.SetNextWindowSizeConstraints(
            new Vector2(Width, 0),
            new Vector2(Width, float.MaxValue));
        ImGui.SetNextWindowBgAlpha(BgAlpha);
    }

    private void DrawTransform(Entity e)
    {
        GUI.SectionTitle("Transform",GUI.Amber);

        var t = e.Transform;
        GUI.Drag3("Position", t.Position, v => t.Position = v);

        if (ImGui.DragFloat3("Rotation", ref _euler, 0.5f))   // cached euler (pitch, yaw, roll)
            t.SetEulerAngles(_euler.X, _euler.Y, _euler.Z);

        GUI.Drag3("Scale", t.Scale, v => t.Scale = v);
        ImGui.Spacing();
    }

    private static void DrawMaterial(Entity e)
    {
        if (e.Material is not { } mat) return;

        GUI.SectionTitle("Material",GUI.Blue);
        GUI.Color4("Color", mat.Color, v => mat.Color = v);
        GUI.Slider("Roughness", mat.RoughnessValue, v => mat.RoughnessValue = v, 0f, 1f); // lower = shinier
        GUI.Slider("Metallic",  mat.MetallicValue,  v => mat.MetallicValue  = v, 0f, 1f);
        ImGui.Spacing();
    }

    private static void DrawLight(Entity e)
    {
        GUI.SectionTitle("Light",GUI.Green);

        var typeIndex = e.Light switch
        {
            DirectionalLight => 1,
            PointLight       => 2,
            SpotLight        => 3,
            _                => 0
        };

        // one control to add / remove / switch the light type
        if (ImGui.Combo("Type", ref typeIndex, LightTypes, LightTypes.Length))
            e.Light = typeIndex == 0 ? null : CreateLight(typeIndex, e.Light);

        if (e.Light is not { } light) return;

        GUI.Check("Light Enabled", light.Enabled, v => light.Enabled = v);
        GUI.Color3("Color##light", light.Color, v => light.Color = v);
        GUI.Drag("Intensity", light.Intensity, v => light.Intensity = v, 0.05f, 0f, 100f);

        switch (light)
        {
            case DirectionalLight d:
                GUI.Drag3("Direction", d.Direction, v => d.Direction = v, 0.01f);
                break;
            case SpotLight s:
                GUI.Drag3("Direction", s.Direction, v => s.Direction = v, 0.01f);
                GUI.Drag("Inner Cutoff", s.InnerCutoff, v => s.InnerCutoff = v, 0.5f, 0f, 90f);
                GUI.Drag("Outer Cutoff", s.OuterCutoff, v => s.OuterCutoff = v, 0.5f, 0f, 90f);
                break;
            case PointLight p:
                GUI.Drag("Linear",    p.Linear,    v => p.Linear    = v, 0.001f, 0f, 1f);
                GUI.Drag("Quadratic", p.Quadratic, v => p.Quadratic = v, 0.001f, 0f, 1f);
                break;
        }

        ImGui.Spacing();
    }

    private static Light CreateLight(int typeIndex, Light? from)
    {
        Light light = typeIndex switch
        {
            1 => new DirectionalLight(),
            2 => new PointLight(),
            3 => new SpotLight(),
            _ => throw new ArgumentOutOfRangeException(nameof(typeIndex))
        };

        if (from is null) return light;

        light.Color     = from.Color;
        light.Intensity = from.Intensity;
        light.Enabled   = from.Enabled;
        return light;
    }
}