namespace Centauri.Rendering.Renderers;

using ImGuiNET;
using System.Numerics;

using Utils.Misc;
using World;

public class StatsOverlay
{
    private ImFontPtr _font = null!;
    public bool IsVisible { get; private set; }

    public void Toggle() => IsVisible = !IsVisible;
    
    private const ImGuiWindowFlags Flags = ImGuiWindowFlags.NoDecoration          |
                                           ImGuiWindowFlags.NoMove                |
                                           ImGuiWindowFlags.NoSavedSettings       |
                                           ImGuiWindowFlags.NoBringToFrontOnFocus |
                                           ImGuiWindowFlags.AlwaysAutoResize;
    
    private readonly Vector4 _defaultColor = new(1.0f, 1.0f, 1.0f, 1.0f);

    public StatsOverlay()
    {
        SetupFont();
    }
    
    private void SetupFont()
    {
        var io = ImGui.GetIO();
        io.Fonts.AddFontDefault();
        
        _font = io.Fonts.AddFontFromFileTTF(PathResolver.Resolve("Assets/Fonts/IosevkaCharon-Regular.ttf"), 18.0f, null);
        io.Fonts.Build();
    }
    
    public void Render(Scene scene, FrameStats stats)
    {
        if (!IsVisible)
            return;

        const float padding = 10f;
        const float width   = 320f;

        var viewport = ImGui.GetMainViewport();
        var pos = new Vector2(
            viewport.WorkPos.X + viewport.WorkSize.X - width - padding,
            viewport.WorkPos.Y + padding
        );

        ImGui.SetNextWindowPos(pos, ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(width, 0), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.85f);

        if (!ImGui.Begin("##StatsOverlay", Flags))
        {
            Console.WriteLine("ImGui couldn't start");
            ImGui.End();
            return;
        }

        var cam = scene.GetActiveCamera();
        
        ImGui.PushFont(_font);
        DrawSection("Performance", new Vector4(1f, 0.8f, 0.2f, 1f), () =>
        {
            Row("FPS", $"{stats.FPS:F1}");
            Row("Frame Time", $"{stats.FrameTime:F2} ms");
        });

        DrawSection("Culling", new Vector4(0.4f, 0.9f, 0.4f, 1f), () =>
        {
            Row("Total", stats.TotalEntities.ToString());

            var drawnColor = stats.CulledEntities > 0
                ? new Vector4(0.4f, 1f, 0.4f, 1f)
                : new Vector4(1f, 1f, 1f, 1f);

            RowColored("Drawn", stats.DrawnEntities.ToString(), drawnColor);
            RowColored("Culled", stats.CulledEntities.ToString(), new Vector4(1f, 0.4f, 0.4f, 1f));
            
            var ratio = stats.TotalEntities > 0
                ? (stats.CulledEntities / (float)stats.TotalEntities) * 100f 
                : 0f;

            RowColored("Ratio", $"{ratio:F2}%", _defaultColor);
        });

        DrawSection("Renderer", new Vector4(0.4f, 0.7f, 1f, 1f), () =>
        {
            Row("Draw Calls", stats.DrawCalls.ToString());
            Row("Texture Binds", stats.TextureBinds.ToString());
        });

        DrawSection("Camera", new Vector4(0.4f, 0.7f, 1f, 1f), () =>
        {
            Row("Active", cam.Name);
            Row("Position", FormatVec3(cam.Position));
            Row("Forward", FormatVec3(cam.Forward));
        });
        ImGui.PopFont();

        ImGui.End();
    }

    // ─────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────

    private void DrawSection(string title, Vector4 color, Action content)
    {
        ImGui.TextColored(color, title);
        ImGui.Separator();

        if (ImGui.BeginTable($"##{title}_table", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 120f);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

            content();

            ImGui.EndTable();
        }

        ImGui.Spacing();
    }

    private void Row(string label, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text(label);

        ImGui.TableSetColumnIndex(1);
        ImGui.Text(value);
    }

    private void RowColored(string label, string value, Vector4 color)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text(label);

        ImGui.TableSetColumnIndex(1);
        ImGui.TextColored(color, value);
    }

    private string FormatVec3(Vector3 v)
    {
        return $"({v.X:F2}, {v.Y:F2}, {v.Z:F2})";
    }
}