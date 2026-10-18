import ChaosGraphics.NormalMap;
import shaders/picking_base.cfx;

int pickingIndex;
float picked;

void fs_Picking(
    vec3 worldPos : TEXCOORD1,
    out vec4 col : COLOR0
) {
    col = vec4(worldPos, pickingIndex);
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
    VertexShader = vs_transformPositionAndWorld;
    FragmentShader = fs_Picking;
}
