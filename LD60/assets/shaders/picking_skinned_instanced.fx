import ChaosGraphics.SkinnedInstancing;
import ChaosGraphics.NormalMap;
import shaders/picking_base.cfx;

void fs_Picking(
    float instanceID : INSTANCE_ID,
    vec3 worldPos : TEXCOORD1,
    vec3 worldNor : TEXCOORD2,
    out vec4 pos : COLOR0,
    out vec4 nor : COLOR1
) {
    pos = vec4(worldPos, instanceID);
    nor = vec4(worldNor, 1);
}

void fs_sampleMaterial(
    vec2 inTex : TEXCOORD0,
    vec2 picked : INSTANCE_ID,
    out vec4 outEmissive : COLOR0,
    out vec4 outDiffuse : COLOR1,
    out vec4 outSpecular : COLOR2
) {
    sampleMaterial(inTex, outEmissive, outDiffuse, outSpecular);
    highlight(picked.y, outEmissive, outDiffuse, outSpecular);
}

Pass Picking
{
    VertexShader = vs_transform, PASS(float INSTANCE_ID);
    FragmentShader = fs_Picking;
}

Pass Material
{
    VertexShader = MATERIAL_VERTEX_SHADER, PASS(vec2 INSTANCE_ID);
    FragmentShader = fs_sampleMaterial;
}
