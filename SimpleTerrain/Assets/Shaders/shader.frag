#version 330 core

const int MAX_POINT_LIGHTS = 16;

struct DirLight {
    vec3  direction;
    vec3  color;
    float intensity;
};

struct PointLight {
    vec3  position;
    vec3  color;
    float intensity;
    float constant;
    float linear;
    float quadratic;
};

uniform sampler2D uTexture0;
uniform vec3      uCameraPos;
uniform DirLight  uDirLight;
uniform PointLight uPointLights[MAX_POINT_LIGHTS];
uniform int       uPointLightCount;

in vec2 fUv;
in vec3 fNormal;
in vec3 fFragPos;

out vec4 FragColor;

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.direction);
    float diff    = max(dot(normal, lightDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec      = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);

    vec3 ambient  = 0.1 * light.color;
    vec3 diffuse  = diff * light.color;
    vec3 specular = spec * light.color * 0.5;

    return (ambient + diffuse + specular) * light.intensity;
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3  lightDir = normalize(light.position - fragPos);
    float diff     = max(dot(normal, lightDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec      = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);

    float dist        = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * dist + light.quadratic * dist * dist);

    vec3 ambient  = 0.1  * light.color;
    vec3 diffuse  = diff * light.color;
    vec3 specular = spec * light.color * 0.5;

    return (ambient + diffuse + specular) * attenuation * light.intensity;
}

void main()
{
    vec4 texColor = texture(uTexture0, fUv);
    if (texColor.a < 0.1) discard;

    vec3 normal  = normalize(fNormal);
    vec3 viewDir = normalize(uCameraPos - fFragPos);

    vec3 result = CalcDirLight(uDirLight, normal, viewDir);

    for (int i = 0; i < uPointLightCount; i++)
    result += CalcPointLight(uPointLights[i], normal, fFragPos, viewDir);

    FragColor = vec4(result * texColor.rgb, texColor.a);
}