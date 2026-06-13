namespace Centauri.Rendering.UI;

using ImGuiNET;
using System.Numerics;

using Utils.Misc;
using World;
using Config;

public class StatsOverlay
{
    private const ImGuiWindowFlags Flags = ImGuiWindowFlags.NoDecoration          |
                                           ImGuiWindowFlags.NoMove                |
                                           ImGuiWindowFlags.NoSavedSettings       |
                                           ImGuiWindowFlags.NoBringToFrontOnFocus |
                                           ImGuiWindowFlags.AlwaysAutoResize;

    private const ImGuiTableFlags TableFlags = ImGuiTableFlags.SizingStretchSame |
                                               ImGuiTableFlags.BordersInnerV;

    private const int   Width   = 350;
    private const float Padding = 10f;
    private const float BgAlpha = 0.85f;

    private readonly ImFontPtr _font;
    private readonly AppConfig _config;

    public StatsOverlay(ImFontPtr font, AppConfig config)
    {
        _font   = font;
        _config = config;
    }

    public void Render(Scene scene, FrameStats stats)
    {
        SetupWindow();

        if (!ImGui.Begin("##StatsOverlay", Flags))
        {
            ImGui.End();
            return;
        }

        var cam = scene.GetActiveCamera();
        ImGui.PushFont(_font);

        Section("Performance",GUI.Amber, () =>
        {
            Row("FPS", GUI.Float(stats.FPS));
            Row("Frame Time", $"{GUI.Float(stats.FrameTime)} ms");
        });

        Section("Culling",GUI.Green, () =>
        {
            Row("Total", stats.TotalEntities.ToString());
            RowColored("Drawn", stats.DrawnEntities.ToString(),
                stats.CulledEntities > 0 ? GUI.Green : GUI.White);
            RowColored("Culled", stats.CulledEntities.ToString(),GUI.Red);

            var ratio = stats.TotalEntities > 0
                ? stats.CulledEntities / (float)stats.TotalEntities * 100f
                : 0f;
            Row("Ratio", $"{GUI.Float(ratio)} %");
        });

        Section("Renderer",GUI.Blue, () =>
        {
            Row("Draw Calls",     stats.DrawCalls.ToString());
            Row("Texture Binds",  stats.TextureBinds.ToString());
            Row("Total Indices",  stats.TotalIndices.ToString());
            Row("Total Vertices", stats.TotalVertices.ToString());
        });

        Section("Camera",GUI.Red, () =>
        {
            RowColored("Active", cam.Name,GUI.Amber);
            RowColored("Position", GUI.Vec3(cam.Position),GUI.Blue);
            RowColored("Forward", GUI.Vec3(cam.Forward),GUI.Green);
            Row("Yaw", GUI.SignedFloat(cam.Yaw));
            Row("Pitch",GUI.SignedFloat(cam.Pitch));
            Row("Zoom", GUI.Float(cam.Zoom));
        });

        Section("Config",GUI.Purple, () =>
        {
            RowColored("ViewMode", _config.Input.Mode.ToString(),GUI.Amber);
            ConfigRow("VSync",         _config.Window.EnableVSync);
            ConfigRow("Culling",       _config.Debug.EnableCulling);
            ConfigRow("DebugView",     _config.Debug.ShowDebugView);
            ConfigRow("BoundingBoxes", _config.Debug.ShowBoundingBoxes);
            ConfigRow("Frustums",      _config.Debug.ShowFrustums);
            ConfigRow("Cameras",       _config.Debug.ShowCameras);
            ConfigRow("Grid",          _config.Debug.ShowGrid);
        });

        ImGui.PopFont();
        ImGui.End();
    }

    private static void SetupWindow()
    {
        var viewport = ImGui.GetMainViewport();
        var anchor = new Vector2(
            viewport.WorkPos.X + viewport.WorkSize.X - Padding,
            viewport.WorkPos.Y + Padding);

        ImGui.SetNextWindowPos(anchor, ImGuiCond.Always, new Vector2(1f, 0f));
        ImGui.SetNextWindowSizeConstraints(
            new Vector2(Width, 0),
            new Vector2(Width, float.MaxValue));
        ImGui.SetNextWindowBgAlpha(BgAlpha);
    }

    private static void Section(string title, Vector4 accent, Action rows)
    {
       GUI.SectionTitle(title, accent);

        if (ImGui.BeginTable($"##{title}", 2, TableFlags))
        {
            ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 120f);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
            rows();
            ImGui.EndTable();
        }

        ImGui.Spacing();
    }

    private static void Row(string label, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(value);
    }

    private static void RowColored(string label, string value, Vector4 color)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);
        ImGui.TableNextColumn();
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(value);
        ImGui.PopStyleColor();
    }

    private static void ConfigRow(string label, bool value) =>
        RowColored(label, value.ToString(),GUI.Bool(value));
}