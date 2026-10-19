import ChaosGraphics.Sprite;

const vec4 HIGHLIGH_COLOR = vec4(1,0,0,1);
const float X = 0.7;

vec4 color;
vec3 info; // time, cooldown, active

float ramp(
    float f
) {
    float t = clamp((f - X) / (1.0 - X), 0.0, 1.0);
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

void fs_sample(
    vec2 inTex : TEXCOORD0,
    out vec4 outColor : COLOR0
) {
    vec4 c = color + (HIGHLIGH_COLOR / 2.0 - color) * info.z * 0.5;

    outColor = textureLod(tex, inTex, 0.0) * c;
    outColor += (HIGHLIGH_COLOR - outColor)
              * info.z
              * ramp(2 * max(abs(0.5 - inTex.x), abs(0.5 - inTex.y)))
              * ((sin(info.x * 6) + 1) * 0.5)
              ;
    // TODO: cooldown
}
