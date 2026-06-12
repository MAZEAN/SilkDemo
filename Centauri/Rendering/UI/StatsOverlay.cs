namespace Centauri.Rendering.UI;

using ImGuiNET;
using System.Numerics;

using Utils.Misc;
using World;
using Config;

public class StatsOverlay
{
    private readonly ImFontPtr _font;
    private readonly AppConfig _config;
    public bool IsVisible { get; private set; }
    public void Toggle() => IsVisible = !IsVisible;

    private const ImGuiWindowFlags Flags = ImGuiWindowFlags.NoDecoration           |
                                           ImGuiWindowFlags.NoMove                 |
                                           ImGuiWindowFlags.NoSavedSettings        |
                                           ImGuiWindowFlags.NoBringToFrontOnFocus;
    
    // Colors
    private static readonly Vector4 DefaultColor = new(1.0f, 1.0f, 1.0f, 1.0f);
    private static readonly Vector4 OrangeColor  = new(1.0f, 0.8f, 0.2f, 1.0f);
    private static readonly Vector4 GreenColor   = new(0.4f, 0.9f, 0.4f, 1.0f);
    private static readonly Vector4 BlueColor    = new(0.4f, 0.7f, 1.0f, 1.0f);
    private static readonly Vector4 RedColor     = new(1.0f, 0.4f, 0.4f, 1.0f);
    private static readonly Vector4 IndigoColor  = new(0.6f, 0.0f, 1.0f, 1.0f);
    
    // Window params
    private const int Width = 350;
    private const float Padding = 10f;
    private const float BgAlpha = 0.85f;

    public StatsOverlay(ImFontPtr font, AppConfig config)
    {
        _font = font;
        _config = config;
    }
    
    public void Render(Scene scene, FrameStats stats)
    {
        if (!IsVisible)
            return;

        SetupWindow();

        if (!ImGui.Begin("##StatsOverlay", Flags))
        {
            Console.WriteLine("ImGui couldn't start");
            ImGui.End();
            return;
        }

        var cam = scene.GetActiveCamera();
        
        ImGui.PushFont(_font);
        DrawSection("Performance", OrangeColor, () =>
        {
            Row("FPS", $"{stats.FPS:F1}");
            Row("Frame Time", $"{stats.FrameTime:F2} ms");
        });

        DrawSection("Culling", GreenColor, () =>
        {
            Row("Total", stats.TotalEntities.ToString());

            var drawnColor = stats.CulledEntities > 0
                ? GreenColor
                : DefaultColor;

            RowColored("Drawn", stats.DrawnEntities.ToString(), drawnColor);
            RowColored("Culled", stats.CulledEntities.ToString(), RedColor);
            
            var ratio = stats.TotalEntities > 0
                ? (stats.CulledEntities / (float)stats.TotalEntities) * 100f 
                : 0f;

            RowColored("Ratio", $"{ratio:F2}%", DefaultColor);
        });

        DrawSection("Renderer", BlueColor, () =>
        {
            Row("Draw Calls", stats.DrawCalls.ToString());
            Row("Texture Binds", stats.TextureBinds.ToString());
            Row("Total Indices", stats.TotalIndices.ToString());
            Row("Total Vertices", stats.TotalVertices.ToString());
        });

        DrawSection("Camera", RedColor, () =>
        {
            RowColored("Active", cam.Name, OrangeColor);
            RowColored("Position", FormatVec3(cam.Position), DefaultColor);
            RowColored("Forward", FormatVec3(cam.Forward),  DefaultColor);
        });
        
        DrawSection("Config", IndigoColor, () =>
        {
            RowColored("VSync", _config.Window.EnableVSync.ToString(),
                ColorBoolean(_config.Window.EnableVSync));
            RowColored("Culling", _config.Debug.EnableCulling.ToString(),
                ColorBoolean(_config.Debug.EnableCulling));
            RowColored("DebugView", _config.Debug.ShowDebugView.ToString(),
                ColorBoolean(_config.Debug.ShowDebugView ));
            RowColored("BoundingBoxes", _config.Debug.ShowBoundingBoxes.ToString(),
                ColorBoolean(_config.Debug.ShowBoundingBoxes));
            RowColored("Frustums", _config.Debug.ShowFrustums.ToString(),
                ColorBoolean(_config.Debug.ShowFrustums));
            RowColored("Cameras", _config.Debug.ShowCameras.ToString(),
                ColorBoolean(_config.Debug.ShowCameras));
            RowColored("Grid", _config.Debug.ShowGrid.ToString(),
                ColorBoolean(_config.Debug.ShowGrid));
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

    private static void DrawSection(string title, Vector4 color, Action content)
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

    private static void Row(string label, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TextUnformatted(label);

        ImGui.TableSetColumnIndex(1);
        ImGui.TextUnformatted(value);
    }

    private static void RowColored(string label, string value, Vector4 color)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TextUnformatted(label);

        ImGui.TableSetColumnIndex(1);
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(value);
        ImGui.PopStyleColor();
    }

    private static Vector4 ColorBoolean(bool value)
    {
        return value ? GreenColor : RedColor;
    }

    private static string FormatVec3(Vector3 v)
    {
        return $"({v.X,8:+0.00;-0.00}, {v.Y,8:+0.00;-0.00}, {v.Z,8:+0.00;-0.00})";
    }
}