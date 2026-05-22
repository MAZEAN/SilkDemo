namespace SimpleTerrain.Core;

using System;

public class MathHelper
{
    public static float DegreesToRadians(float degrees)
    {
        return MathF.PI / 180f * degrees;
    }
}