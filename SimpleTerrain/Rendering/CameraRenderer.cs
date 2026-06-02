namespace SimpleTerrain.Rendering;

using Silk.NET.OpenGL;
using System.Numerics;
using SimpleTerrain.Scene;

public class CameraRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly GLShader _shader;
    private readonly Mesh _mesh;

    public CameraRenderer(GL gl)
    {
        _gl = gl;
        _shader = new GLShader(gl, "Assets/Shaders/debug.vert", "Assets/Shaders/debug.frag");

        // simple pyramid (camera shape)
        float[] vertices =
        [
            // pos              normal  uv   tangent
             0, 0, 0,           0,1,0,  0,0,  1,0,0,
            -0.2f,-0.2f,-0.4f,  0,1,0,  0,0,  1,0,0,
             0.2f,-0.2f,-0.4f,  0,1,0,  0,0,  1,0,0,
             0.2f, 0.2f,-0.4f,  0,1,0,  0,0,  1,0,0,
            -0.2f, 0.2f,-0.4f,  0,1,0,  0,0,  1,0,0
        ];

        uint[] indices =
        [
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 1,
            1, 2, 3,
            3, 4, 1
        ];

        _mesh = new Mesh(gl, vertices, indices);
    }

    public void Render(Scene scene)
    {
        SetDebugRenderState();
        
        var activeCamera = scene.GetActiveCamera();
        
        _shader.Use();

        _shader.SetUniform("uView", activeCamera.GetViewMatrix());
        _shader.SetUniform("uProjection", activeCamera.GetProjectionMatrix());

        foreach (var cam in scene.Cameras)
        {
            if (cam == activeCamera)
                continue;

            RenderCamera(cam);
        }
        
        RestoreRenderState();
    }

    private void RenderCamera(Camera cam)
    {
        var model =
            Matrix4x4.CreateScale(0.5f) *
            Matrix4x4.CreateWorld(cam.Position, cam.Forward, cam.Up);

        _shader.SetUniform("uModel", model);

        _mesh.Bind();

        unsafe
        {
            _gl.DrawElements(
                PrimitiveType.Triangles,
                _mesh.IndexCount,
                DrawElementsType.UnsignedInt,
                (void*)0
            );
        }
    }
    
    private void SetDebugRenderState()
    {
        _gl.Disable(EnableCap.CullFace);
        _gl.Disable(EnableCap.DepthTest);
    }

    private void RestoreRenderState()
    {
        _gl.Enable(EnableCap.CullFace);
        _gl.Enable(EnableCap.DepthTest);
    }


    public void Dispose()
    {
        _mesh.Dispose();
        _shader.Dispose();
    }
}