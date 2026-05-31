namespace SimpleTerrain.Scene;

using System.Numerics;

public class Transform
{
    private Vector3 _position = Vector3.Zero;
    private Vector3 _scale = Vector3.One;
    private Quaternion _rotation = Quaternion.Identity;
    private Matrix4x4 _cachedMatrix;
    private bool _dirty = true;

    public Transform? Parent { get; set; }
    
    public bool IsDirty => _dirty;
    public void ClearDirty() => _dirty = false;

    public Vector3 Position
    {
        get => _position;
        set { _position = value; _dirty = true; }
    }

    public Vector3 Scale
    {
        get => _scale;
        set { _scale = value; _dirty = true; }
    }

    public Quaternion Rotation
    {
        get => _rotation;
        set { _rotation = Quaternion.Normalize(value); _dirty = true; }
    }

    public Matrix4x4 LocalMatrix
    {
        get
        {
            if (_dirty)
            {
                _cachedMatrix =
                    Matrix4x4.CreateScale(_scale) *
                    Matrix4x4.CreateFromQuaternion(_rotation) *
                    Matrix4x4.CreateTranslation(_position);
                _dirty = false;
            }
            return _cachedMatrix;
        }
    }

    public Matrix4x4 WorldMatrix =>
        Parent != null
            ? LocalMatrix * Parent.WorldMatrix
            : LocalMatrix;

    public void Translate(Vector3 delta)
    {
        Position += delta;
    }

    public void RotateLocal(Quaternion delta)
    {
        Rotation = delta * _rotation;
    }

    public void RotateWorld(Quaternion delta)
    {
        Rotation = _rotation * delta;
    }

    public void SetEulerAngles(float pitchDeg, float yawDeg, float rollDeg)
    {
        Rotation = Quaternion.CreateFromYawPitchRoll(
            float.DegreesToRadians(yawDeg),
            float.DegreesToRadians(pitchDeg),
            float.DegreesToRadians(rollDeg)
        );
    }
}