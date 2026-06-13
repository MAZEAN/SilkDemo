namespace Centauri.Rendering.UI;

using ImGuiNET;
using System.Numerics;
using System.Globalization;

// Shared visual language + widget helpers for the engine's ImGui panels.
internal static class GUI
{
    // ── palette ────────────────────────────────────────────────────────────────
    public static readonly Vector4 Amber   = new(1.00f, 0.75f, 0.20f, 1f);
    public static readonly Vector4 Green   = new(0.45f, 0.90f, 0.45f, 1f);
    public static readonly Vector4 Blue    = new(0.40f, 0.70f, 1.00f, 1f);
    public static readonly Vector4 Red     = new(1.00f, 0.35f, 0.35f, 1f);
    public static readonly Vector4 Purple  = new(0.70f, 0.50f, 1.00f, 1f);
    public static readonly Vector4 White   = Vector4.One;

    public static Vector4 Bool(bool value) => value ? Green : Red;

    // ── section header (identical across panels) ────────────────────────────────
    public static void SectionTitle(string title, Vector4 accent)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, accent);
        ImGui.TextUnformatted(title);
        ImGui.PopStyleColor();
        ImGui.Separator();
    }

    // ── bound widgets — read current value, write back only on change ───────────
    public static void Drag3(string label, Vector3 v, Action<Vector3> set, float speed = 0.05f)
    {
        if (ImGui.DragFloat3(label, ref v, speed)) set(v);
    }

    public static void Drag(string label, float v, Action<float> set, float speed, float min, float max)
    {
        if (ImGui.DragFloat(label, ref v, speed, min, max)) set(v);
    }

    public static void Slider(string label, float v, Action<float> set, float min, float max)
    {
        if (ImGui.SliderFloat(label, ref v, min, max)) set(v);
    }

    public static void Color4(string label, Vector4 v, Action<Vector4> set)
    {
        if (ImGui.ColorEdit4(label, ref v)) set(v);
    }

    public static void Color3(string label, Vector3 v, Action<Vector3> set)
    {
        if (ImGui.ColorEdit3(label, ref v)) set(v);
    }

    public static void Check(string label, bool v, Action<bool> set)
    {
        if (ImGui.Checkbox(label, ref v)) set(v);
    }

    // ── formatting ──────────────────────────────────────────────────────────────
    public static string Vec3(Vector3 v) => string.Format(
        CultureInfo.CurrentCulture,
        "({0,8:+0.00;-0.00}, {1,8:+0.00;-0.00}, {2,8:+0.00;-0.00})", v.X, v.Y, v.Z);

    public static string Float(float v, int decimals = 2) =>
        v.ToString($"F{decimals}", CultureInfo.CurrentCulture);

    public static string SignedFloat(float v, int decimals = 2) =>
        v.ToString($"+0.{new string('0', decimals)};-0.{new string('0', decimals)}", CultureInfo.CurrentCulture);
}