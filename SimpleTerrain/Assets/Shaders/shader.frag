#version 330 core
in vec2 fUv;

uniform sampler2D uTexture0;

out vec4 FragColor;

void main()
{
    vec4 color = texture(uTexture0, fUv);

    if (color.a < 0.1)
        discard;

    FragColor = color;
}