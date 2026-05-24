#version 330 core
in vec3 fNearPoint;
in vec3 fFarPoint;

uniform mat4 uView;
uniform mat4 uProjection;

out vec4 FragColor;

vec4 Grid(vec3 fragPos, float scale, bool drawAxis)
{
    vec2 coord     = fragPos.xz * scale;
    vec2 derivative = fwidth(coord);
    vec2 grid      = abs(fract(coord - 0.5) - 0.5) / derivative;
    float line     = min(grid.x, grid.y);
    float minZ     = min(derivative.y, 1.0);
    float minX     = min(derivative.x, 1.0);
    vec4 color = vec4(0.4, 0.4, 0.4, 1.0 - min(line / 1.5, 1.0));

    if (drawAxis)
    {
        // Z axis — blue
        if (fragPos.x > -0.1 * minX && fragPos.x < 0.1 * minX)
        color.z = 1.0;
        // X axis — red
        if (fragPos.z > -0.1 * minZ && fragPos.z < 0.1 * minZ)
        color.x = 1.0;
    }

    return color;
}

float ComputeDepth(vec3 pos)
{
    vec4 clipSpacePos = uProjection * uView * vec4(pos, 1.0);
    return clipSpacePos.z / clipSpacePos.w;
}

float ComputeFade(vec3 pos)
{
    vec4 clipSpacePos = uProjection * uView * vec4(pos, 1.0);
    float depth = clipSpacePos.z / clipSpacePos.w;
    return max(0.0, 0.5 - depth * 0.5);
}

void main()
{
    float t = -fNearPoint.y / (fFarPoint.y - fNearPoint.y);
    if (t < 0.0) discard;

    vec3 fragPos = fNearPoint + t * (fFarPoint - fNearPoint);

    gl_FragDepth = ComputeDepth(fragPos);

    float fade = ComputeFade(fragPos);

    FragColor = (Grid(fragPos, 1.0, true) + Grid(fragPos, 0.1, false)) * vec4(1.0, 1.0, 1.0, fade);

    if (FragColor.a < 0.01) discard;
}