import ChaosGraphics.NormalMap;
import shaders/picking_base.cfx;

int pickingIndex;
float picked;

void fs_Picking(
    out vec4 col : COLOR0
) {
    col = vec4(pickingIndex, 0.0, 0.0, 1.0);
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
    VertexShader = vs_transformPosition;
    FragmentShader = fs_Picking;
}
