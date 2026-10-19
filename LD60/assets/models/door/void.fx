import ChaosGraphics.MaterialDefault;
import ChaosGraphics.Instancing;
import ChaosGraphics.NormalMapPasses;

#define TAU 6.28318530718

mat2 rotate2D(
    float a
) {
    float c = cos(a);
    float s = sin(a);
    return mat2(c, -s, s, c);
}

float hash(
    vec2 p
) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float noise(
    vec2 p
) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);

    return mix(
        mix(hash(i), hash(i + vec2(1.0, 0.0)), f.x),
        mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0, 1.0)), f.x),
        f.y
    );
}

float fbm(
    vec2 p
) {
    float sum = 0.0;
    float amp = 0.5;

    for (int i = 0; i < 5; i++)
    {
        sum += amp * noise(p);
        p = rotate2D(0.55) * p * 2.03 + 13.7;
        amp *= 0.5;
    }

    return sum;
}

void fs_sampleMaterial(
    vec2 uv : TEXCOORD0,
    float t : TIME,
    out vec4 outEmissive : COLOR0,
    out vec4 outDiffuse  : COLOR1,
    out vec4 outSpecular : COLOR2
) {
    uv -= 0.5;

    float r = length(uv);
    float a = atan(uv.y, uv.x);

    // Distort the portal boundary so it churns.
    float edge = 0.42
        + 0.025 * sin(a * 7.0 + t * 1.4)
        + 0.018 * sin(a * 13.0 - t * 2.1);

    float inside = 1.0 - smoothstep(edge - 0.01, edge + 0.01, r);
    float rim = exp(-abs(r - edge) * 45.0);

    // Swirling bands with time-varying distortion.
    float twist = a + 2.0 / (r + 0.12) - t;
    float flow = sin(18.0 * r - 3.0 * t + 2.5 * sin(twist * 2.0));
    float wisps = smoothstep(-0.25, 0.8, flow);

    vec3 color = vec3(0.012, 0.004, 0.035);
    color += inside * wisps * vec3(0.22, 0.06, 0.75);
    color += inside * wisps * vec3(0.04, 0.45, 0.8) * (0.5 + 0.5 * sin(twist * 3.0));
    color += rim * vec3(0.25, 0.75, 1.0) * 2.0;

    outEmissive = vec4(color, 1.0);
    outSpecular = vec4(0.0, 0.0, 0.0, 1.0);
    outDiffuse = vec4(0.0, 0.0, 0.0, 1.0);
}

Pass Material
{
    VertexShader = MATERIAL_VERTEX_SHADER, PASS(float TIME);
    FragmentShader = fs_sampleMaterial;
}
