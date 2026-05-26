#version 330 core

// ─── Blender-style grid ───────────────────────────────────────────────────────
const vec3  GRID_COLOR_COARSE = vec3(0.35, 0.35, 0.35);
const vec3  GRID_COLOR_FINE   = vec3(0.22, 0.22, 0.22);
const float GRID_THICKNESS    = 1.0;
const float GRID_UNIT = 10.0;

const vec3  AXIS_COLOR_X      = vec3(0.87, 0.17, 0.17);
const vec3  AXIS_COLOR_Z      = vec3(0.17, 0.40, 0.87);
const float AXIS_WIDTH        = 0.1;

const float ALPHA_THRESHOLD   = 0.005;
// ─────────────────────────────────────────────────────────────────────────────

in vec3 fNearPoint;
in vec3 fFarPoint;

uniform mat4 uView;
uniform mat4 uProjection;
uniform vec3 uCameraPos;

out vec4 FragColor;

float log10(float x)
{
    return log(x) / log(10.0);
}

vec4 Grid(vec3 fragPos, float scale, vec3 gridColor, bool drawAxis)
{
    vec2 coord      = fragPos.xz * scale;
    vec2 derivative = fwidth(coord);
    vec2 grid       = abs(fract(coord - 0.5) - 0.5) / derivative;
    float line      = min(grid.x, grid.y);
    float minZ      = min(derivative.y, 1.0);
    float minX      = min(derivative.x, 1.0);

    float alpha = 1.0 - min(line / GRID_THICKNESS, 1.0);
    vec4 color  = vec4(gridColor, alpha);

    if (drawAxis)
    {
        if (fragPos.x > -AXIS_WIDTH * minX && fragPos.x < AXIS_WIDTH * minX)
        color = vec4(AXIS_COLOR_Z, alpha);
        if (fragPos.z > -AXIS_WIDTH * minZ && fragPos.z < AXIS_WIDTH * minZ)
        color = vec4(AXIS_COLOR_X, alpha);
    }

    return color;
}

float ComputeDepth(vec3 pos)
{
    vec4 clipSpacePos = uProjection * uView * vec4(pos, 1.0);
    return clipSpacePos.z / clipSpacePos.w;
}

float ComputeGridScale()
{
    float dist = length(uCameraPos) / GRID_UNIT;

    float logScale = log10(dist + 1.0);
    return pow(10.0, floor(logScale));
}

float ComputeFade(float scale)
{
    float height = abs(uCameraPos.y) / GRID_UNIT;

    float fadeStart = scale * 2.0;
    float fadeEnd   = scale * 20.0;

    return 1.0 - smoothstep(fadeStart, fadeEnd, height);
}

void main()
{
    float t = -fNearPoint.y / (fFarPoint.y - fNearPoint.y);
    if (t < 0.0) discard;

    vec3 fragPos = fNearPoint + t * (fFarPoint - fNearPoint);

    gl_FragDepth = ComputeDepth(fragPos);

    float scale = ComputeGridScale();
    float fade  = ComputeFade(scale);

    vec4 coarse = Grid(fragPos, 1.0 / (scale * GRID_UNIT),  GRID_COLOR_COARSE, true);
    vec4 fine   = Grid(fragPos, 10.0 / (scale * GRID_UNIT), GRID_COLOR_FINE,  false);

    FragColor = vec4(max(coarse, fine).rgb, max(coarse, fine).a * fade);

    if (FragColor.a < ALPHA_THRESHOLD) discard;
}