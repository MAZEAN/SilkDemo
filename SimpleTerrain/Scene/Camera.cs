namespace SimpleTerrain.Scene;

using System;
using System.Numerics;
using Silk.NET.Maths;
using Config;
using Core;

public class Camera
{
    private readonly CameraConfig _config;
    private Vector3 _position; 
    
    public Vector3 Forward { get; private set; }
    public Vector3 Up { get; private set; }
    
    private float _aspectRatio;
    private float _yaw;
    private float _pitch;
    private float _zoom;

    public Camera(CameraConfig config, Vector3 position, Vector3 forward, Vector3 up, float yaw, float pitch)
    {
        _config = config;
        _position = position;
        Forward = forward;
        Up = up;
        _yaw = yaw;
        _pitch = pitch;
        _zoom = _config.FOV;
    }

    public void AdjustZoom(float zoomDelta)
    {
        _zoom = Math.Clamp(_zoom + zoomDelta, 1.0f, _config.FOV);
    }

    public void SetAspectRatio(Vector2D<int> newSize)
    {
        _aspectRatio = (float) newSize.X / newSize.Y;
    }

    public void UpdatePosition(Vector3 delta)
    {
        _position += delta;
    }

    public void ModifyDirection(float xOffset, float yOffset)
    {
        _yaw += xOffset;
        _pitch -= yOffset;

        // We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
        _pitch = Math.Clamp(_pitch, -89f, 89f);

        var cameraDirection = Vector3.Zero;
        cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
        cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));

        Forward = Vector3.Normalize(cameraDirection);
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(_position, _position + Forward, Up);
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_zoom), _aspectRatio, _config.Near, _config.Far);
    }

    public Vector3 GetPosition()
    {
        return _position;
    }
}