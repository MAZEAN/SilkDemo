namespace SimpleTerrain.Scene;

using System;
using System.Numerics;
using Silk.NET.Maths;
using Config;
using Core;

public class Camera
{
    public CameraConfig Config { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Front { get; set; }
    public Vector3 Up { get; private set; }
    public float AspectRatio { get; private set; }
    public float Yaw { get; private set; }
    public float Pitch { get; private set; }
    public float Zoom { get; private set; }
    
    public float LookSensitivityX { get; private set; } = 0.1f;
    public float LookSensitivityY { get; private set; } = 0.1f;

    public Camera(CameraConfig config, Vector3 position, Vector3 front, Vector3 up, float yaw, float pitch)
    {
        Config = config;
        Position = position;
        Front = front;
        Up = up;
        Yaw = yaw;
        Pitch = pitch;
        Zoom = Config.FOV;
    }

    public void AdjustZoom(float zoomAmount)
    {
        //We don't want to be able to zoom in too close or too far away so clamp to these values
        Zoom = Math.Clamp(Zoom - zoomAmount, 1.0f, Config.FOV);
    }

    public void SetAspectRatio(Vector2D<int> newSize)
    {
        AspectRatio = (float) newSize.X / newSize.Y;
    }

    public void UpdatePosition(Vector3 delta)
    {
        Position += delta;
    }

    public void ModifyDirection(float xOffset, float yOffset)
    {
        Yaw += xOffset;
        Pitch -= yOffset;

        //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
        Pitch = Math.Clamp(Pitch, -89f, 89f);

        var cameraDirection = Vector3.Zero;
        cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
        cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
        cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));

        Front = Vector3.Normalize(cameraDirection);
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Position + Front, Up);
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Zoom), AspectRatio, Config.Near, Config.Far);
    }
}