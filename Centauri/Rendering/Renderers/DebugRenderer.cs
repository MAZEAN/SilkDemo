namespace Centauri.Rendering.Renderers;

using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.CompilerServices;

using World;
using Utils.Geometry;
using Config;

public sealed class DebugRenderer : IDisposable
{
    private readonly AppConfig _config;
    private readonly DebugDraw _draw;
    private readonly Geometry.Mesh _cameraMesh;

    private bool _active;

    private const float DirLineLength = 100.0f;
    private const float FaceAlpha     = 0.05f; // translucency of AABB side faces

    private static readonly Vector3 ColorCamera     = new(1.0f, 0.5f, 0.0f);
    private static readonly Vector3 ColorDir        = new(1.0f, 1.0f, 1.0f);
    private static readonly Vector3 ColorFrustum    = new(1.0f, 1.0f, 0.0f);
    private static readonly Vector3 ColorAABB       = new(0.0f, 1.0f, 0.0f);
    private static readonly Vector3 ColorAABBCulled = new(1.0f, 0.0f, 0.0f);

    public DebugRenderer(GL gl, AppConfig config)
    {
        _config     = config;
        _draw       = new DebugDraw(gl);
        _cameraMesh = DebugShapes.BuildCameraMesh(gl);
    }

    // ── Begin / End ───────────────────────────────────────────────────────────
    public void Begin(Camera camera)
    {
        if (_active)
            throw new InvalidOperationException("DebugRenderer.Begin called twice without End.");

        _active = true;
        _draw.Begin(camera.GetViewMatrix(), camera.GetProjectionMatrix());
    }

    public void End()
    {
        if (!_active)
            throw new InvalidOperationException("DebugRenderer.End called without Begin.");

        _active = false;
        _draw.End();
    }

    // ── Draw calls — must be between Begin/End ────────────────────────────────
    public void DrawCameras(Scene scene)
    {
        AssertActive();
        if (!_config.Debug.ShowCameras) return;

        var active = scene.GetActiveCamera();

        foreach (var cam in scene.Cameras)
        {
            if (cam == active) continue;

            DrawCameraShape(cam);
            DrawDirectionLine(cam);

            if (_config.Debug.ShowFrustums)
                DrawFrustum(cam);
        }
    }

    public void DrawAllAABBs(Scene scene, Frustum cullingFrustum)
    {
        AssertActive();
        if (!_config.Debug.ShowBoundingBoxes) return;

        _draw.Model(Matrix4x4.Identity);

        foreach (var entity in scene.Entities)
        {
            var bounds  = entity.GetWorldBounds();
            var culled  = !cullingFrustum.IsVisibleAABB(bounds);
            var corners = bounds.GetBoxCorners();
            var color   = culled ? ColorAABBCulled : ColorAABB;

            _draw.Color(color, FaceAlpha);          // translucent fill
            _draw.EnableDepthMask();
            _draw.Triangles(DebugShapes.BoxFaces(corners));
            _draw.DisableDepthMask();

            _draw.Color(color, 1.0f);               // opaque wireframe on top
            _draw.Lines(DebugShapes.BoxEdges(corners));
        }
    }

    // ── Private drawing ───────────────────────────────────────────────────────
    private void DrawCameraShape(Camera cam)
    {
        var model =
            Matrix4x4.CreateScale(DebugShapes.CameraScale) *
            Matrix4x4.CreateWorld(cam.Position, cam.Forward, cam.Up);

        _draw.Model(model);
        _draw.Color(ColorCamera);
        _draw.DrawMesh(_cameraMesh);
    }

    private void DrawDirectionLine(Camera cam)
    {
        _draw.Model(Matrix4x4.Identity);
        _draw.Color(ColorDir);

        var tipOffset = MathF.Abs(DebugShapes.CameraModelBase) * DebugShapes.CameraScale;
        var start     = cam.Position + cam.Forward * tipOffset;
        var end       = start + cam.Forward * DirLineLength;

        _draw.Lines([start.X, start.Y, start.Z, end.X, end.Y, end.Z]);
    }

    private void DrawFrustum(Camera cam)
    {
        _draw.Model(Matrix4x4.Identity);
        _draw.Color(ColorFrustum);
        _draw.Lines(DebugShapes.BoxEdges(cam.Frustum.GetFrustumCorners()));
    }

    private void AssertActive([CallerMemberName] string caller = "")
    {
        if (!_active)
            throw new InvalidOperationException(
                $"DebugRenderer.{caller} called outside Begin/End block.");
    }

    public void Dispose()
    {
        _cameraMesh.Dispose();
        _draw.Dispose();
    }
}