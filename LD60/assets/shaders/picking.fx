import ChaosGraphics.NormalMap;
import shaders/picking_base.cfx;

int pickingIndex;
float picked;

void fs_Picking(
    vec3 worldPos : TEXCOORD1,
    vec3 worldNor : TEXCOORD2,
    out vec4 pos : COLOR0,
    out vec4 nor : COLOR1
) {
    pos = vec4(worldPos, pickingIndex);
    nor = vec4(worldNor, 1);
}

void fs_sampleMaterial(
    vec2 inTex : TEXCOORD0,
    out vec4 outEmissive : COLOR0,
    out vec4 outDiffuse : COLOR1,
    out vec4 outSpecular : COLOR2
) {
    sampleMaterial(inTex, outEmissive, outDiffuse, outSpecular);
    highlight(picked, outEmissive, outDiffuse, outSpecular);
}

Pass Picking
{
    VertexShader = vs_transformPositionAndWorld, vs_transformNormal;
    FragmentShader = fs_Picking;
}
