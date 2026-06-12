namespace Centauri.Utils.Geometry;

using System.Numerics;

public readonly struct Ray(Vector3 origin, Vector3 direction)
{
    public Vector3 Origin    { get; } = origin;
    public Vector3 Direction { get; } = Vector3.Normalize(direction);
}