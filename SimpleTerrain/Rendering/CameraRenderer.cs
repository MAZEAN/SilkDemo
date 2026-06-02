namespace SimpleTerrain.Rendering;

using Silk.NET.OpenGL;
using System.Numerics;
using Scene;

public class CameraRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly GLShader _shader;
    private readonly Mesh _mesh;
    
    private uint _lineVao;
    private uint _lineVbo;

    private readonly Vector3 _cameraColor = new(1.0f, 0.5f, 0.0f);
    private readonly Vector3 _dirColor = new(1.0f, 1.0f, 1.0f);

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
        
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        _lineVao = _gl.GenVertexArray();
        _lineVbo = _gl.GenBuffer();

        _gl.BindVertexArray(_lineVao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _lineVbo);
        
        unsafe
        {
            _gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(2 * 3 * sizeof(float)), 
                null,
                BufferUsageARB.DynamicDraw
            );
        }

        _gl.EnableVertexAttribArray(0);
        unsafe
        {
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
        }

        _gl.BindVertexArray(0);
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
            DrawDirectionLine(cam);
        }
        
        RestoreRenderState();
    }

    private void RenderCamera(Camera cam)
    {
        var model =
            Matrix4x4.CreateScale(0.5f) *
            Matrix4x4.CreateWorld(cam.Position, cam.Forward, cam.Up);

        _shader.SetUniform("uModel", model);
        _shader.SetUniform("uColor", _cameraColor);
        
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
    
    private void DrawDirectionLine(Camera cam)
    {
        _shader.SetUniform("uModel", Matrix4x4.Identity);
        _shader.SetUniform("uColor", _dirColor);

        
        float tipOffset = 0.4f * 0.5f; 
        Vector3 start = cam.Position + cam.Forward * tipOffset;
        Vector3 end   = cam.Position + cam.Forward * 0.5f;

        float[] vertices =
        [
            start.X, start.Y, start.Z,
            end.X,   end.Y,   end.Z
        ];
        
        _gl.BindVertexArray(_lineVao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _lineVbo);

        unsafe
        {
            fixed (float* v = vertices)
            {
                _gl.BufferSubData(
                    BufferTargetARB.ArrayBuffer,
                    0,
                    (nuint)(vertices.Length * sizeof(float)),
                    v
                );
            }
        }

        _gl.BindVertexArray(_lineVao);

        _gl.DrawArrays(PrimitiveType.Lines, 0, 2);
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
        _gl.DeleteBuffer(_lineVbo);
        _gl.DeleteVertexArray(_lineVao);
        
        _mesh.Dispose();
        _shader.Dispose();
    }
}